using System;
using NullGuard;

namespace GitHub.Models
{
    [NullGuard(ValidationFlags.None)]
    public class PullRequestReviewCommentModel : IPullRequestReviewCommentModel
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public int? Position { get; set; }
        public int? OriginalPosition { get; set; }
        public string CommitId { get; set; }
        public string OriginalCommitId { get; set; }
        public string DiffHunk { get; set; }
        public IAccount User { get; set; }
        public string Body { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
