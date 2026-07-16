using System;
using System.Collections.Generic;
using PollBuilder.API.Data;

namespace PollBuilder.API.DTOs
{
    public record CreatePollRequest(string Question, QuestionType Type, List<string> Options, DateTime? ExpiresAt);
    public record VoteRequest(int? OptionIndex, string TextResponse, string VoterToken);
    public record SubmitQuestionRequest(string Content);

    public record PollResultSummary(
        string Code,
        string Question,
        QuestionType Type,
        bool IsClosed,
        List<OptionCount> Distribution,
        List<string> OpenTextResponses,
        List<QuestionDto> Questions
    );

    public record OptionCount(string Option, int Count);
    public record QuestionDto(int Id, string Content, int Upvotes, bool IsPinned);
}