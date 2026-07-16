using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PollBuilder.API.Data;
using PollBuilder.API.DTOs;
using PollBuilder.API.Hubs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PollBuilder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PollsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<PollHub> _hubContext;

        public PollsController(AppDbContext context, IHubContext<PollHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePollRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("Question cannot be blank.");

            var code = Guid.NewGuid().ToString("N")[..6]; // Simple short unique slug

            var poll = new Poll
            {
                Code = code,
                Question = request.Question,
                Type = request.Type,
                Options = request.Type == QuestionType.YesNo ? new() { "Yes", "No" } : request.Options,
                ExpiresAt = request.ExpiresAt,
                IsClosed = false
            };

            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByCode), new { code = poll.Code }, poll);
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var poll = await _context.Polls
                .Select(p => new { p.Code, p.Question, p.Type, p.Options, p.IsClosed, p.ExpiresAt })
                .FirstOrDefaultAsync(p => p.Code == code);

            if (poll == null) return NotFound();

            // Check dynamic expiration window instantly on query
            bool systemClosed = poll.IsClosed || (poll.ExpiresAt.HasValue && poll.ExpiresAt.Value < DateTime.UtcNow);

            return Ok(new { poll.Code, poll.Question, poll.Type, poll.Options, IsClosed = systemClosed });
        }

        [HttpPost("{code}/vote")]
        public async Task<IActionResult> Vote(string code, [FromBody] VoteRequest request)
        {
            var poll = await _context.Polls.FirstOrDefaultAsync(p => p.Code == code);
            if (poll == null) return NotFound();

            if (poll.IsClosed || (poll.ExpiresAt.HasValue && poll.ExpiresAt.Value < DateTime.UtcNow))
                return BadRequest("Voting is closed for this poll.");

            bool baseCheck = await _context.Votes.AnyAsync(v => v.PollCode == code && v.VoterToken == request.VoterToken);
            if (baseCheck) return Conflict("You have already cast a vote on this poll.");

            var vote = new Vote
            {
                PollCode = code,
                OptionIndex = request.OptionIndex,
                TextResponse = request.TextResponse,
                VoterToken = request.VoterToken
            };

            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            // Push state synchronization payload out over WebSockets
            var summary = await GetSummaryData(code);
            await _hubContext.Clients.Group(code).SendAsync("BroadcastUpdate", summary);

            return Ok();
        }

        [HttpPost("{code}/close")]
        public async Task<IActionResult> ForceClose(string code)
        {
            var poll = await _context.Polls.FirstOrDefaultAsync(p => p.Code == code);
            if (poll == null) return NotFound();

            poll.IsClosed = true;
            await _context.SaveChangesAsync();

            var summary = await GetSummaryData(code);
            await _hubContext.Clients.Group(code).SendAsync("BroadcastUpdate", summary);

            return Ok();
        }

        [HttpPost("{code}/questions")]
        public async Task<IActionResult> SubmitLiveQuestion(string code, [FromBody] SubmitQuestionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content)) return BadRequest();

            var textQuestion = new TextQuestion { PollCode = code, Content = request.Content };
            _context.TextQuestions.Add(textQuestion);
            await _context.SaveChangesAsync();

            var summary = await GetSummaryData(code);
            await _hubContext.Clients.Group(code).SendAsync("BroadcastUpdate", summary);

            return Ok();
        }

        [HttpGet("{code}/results")]
        public async Task<IActionResult> GetResults(string code)
        {
            var summary = await GetSummaryData(code);
            if (summary == null) return NotFound();
            return Ok(summary);
        }

        private async Task<PollResultSummary?> GetSummaryData(string code)
        {
            var poll = await _context.Polls
                .FirstOrDefaultAsync(p => p.Code == code);

            if (poll == null) return null;

            // 1. Get raw vote counts grouped by their index straight from the DB
            var voteCounts = await _context.Votes
                .Where(v => v.PollCode == code && v.OptionIndex != null) // 🟢 Add this filter!
                .GroupBy(v => v.OptionIndex)
                .Select(g => new { OptionIndex = g.Key!.Value, Count = g.Count() }) // 🟢 Cast it to a non-nullable int
                .ToDictionaryAsync(x => x.OptionIndex, x => x.Count);

            // 2. Safely map options in memory using the local dictionary
            var distribution = poll.Options.Select((opt, idx) => new OptionCount(
                opt,
                voteCounts.TryGetValue(idx, out var count) ? count : 0
            )).ToList();

            // 3. Fetch text responses safely
            var textResponses = await _context.Votes
                .Where(v => v.PollCode == code && !string.IsNullOrEmpty(v.TextResponse))
                .Select(v => v.TextResponse!)
                .ToListAsync();

            // 4. Fetch live text stream questions safely
            var questions = await _context.TextQuestions
                .Where(q => q.PollCode == code)
                .OrderByDescending(q => q.IsPinned)
                .ThenByDescending(q => q.Upvotes)
                .Select(q => new QuestionDto(q.Id, q.Content, q.Upvotes, q.IsPinned))
                .ToListAsync();

            bool systemClosed = poll.IsClosed || (poll.ExpiresAt.HasValue && poll.ExpiresAt.Value < DateTime.UtcNow);

            return new PollResultSummary(poll.Code, poll.Question, poll.Type, systemClosed, distribution, textResponses, questions);
        }
    }
}