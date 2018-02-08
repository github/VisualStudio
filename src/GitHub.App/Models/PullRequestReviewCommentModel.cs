using System;

namespace GitHub.Models
{
    public class PullRequestReviewCommentModel : IPullRequestReviewCommentModel
    {
        public int Id { get; set; }
        public string NodeId { get; set; }
        public long PullRequestReviewId { get; set; }
        public string Path { get; set; }
        public int? Position { get; set; }
        public int? OriginalPosition { get; set; }
        public string CommitId { get; set; }
        public string OriginalCommitId { get; set; }
        public string DiffHunk { get; set; }
        public IAccount User { get; set; }
        public string Body { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool IsPending { get; set; }
    }
}
