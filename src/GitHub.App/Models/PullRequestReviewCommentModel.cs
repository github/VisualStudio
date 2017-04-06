using System;
using NullGuard;

namespace GitHub.Models
{
    [NullGuard(ValidationFlags.None)]
    public class PullRequestReviewCommentModel : IPullRequestReviewCommentModel
    {
        public string Path { get; set; }
        public int? Position { get; set; }
        public IAccount User { get; set; }
        public string Body { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
