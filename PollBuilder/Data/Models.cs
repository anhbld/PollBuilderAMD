using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PollBuilder.API.Data
{
    public enum QuestionType
    {
        MultipleChoice,
        YesNo,
        Rating,
        OpenText
    }

    public class Poll
    {
        [Key]
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Question { get; set; } = string.Empty;

        public QuestionType Type { get; set; } = QuestionType.MultipleChoice;

        public List<string> Options { get; set; } = new();

        public bool IsClosed { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public ICollection<TextQuestion> TextQuestions { get; set; } = new List<TextQuestion>();
    }

    public class Vote
    {
        public int Id { get; set; }

        [Required]
        public string PollCode { get; set; } = string.Empty;
        public Poll Poll { get; set; } = null!;

        public int? OptionIndex { get; set; } // Null for pure open-text questions
        public string? TextResponse { get; set; } // Used for open-text mode

        [Required]
        public string VoterToken { get; set; } = string.Empty;

        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    }

    public class TextQuestion
    {
        public int Id { get; set; }

        [Required]
        public string PollCode { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public int Upvotes { get; set; }
        public bool IsPinned { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}