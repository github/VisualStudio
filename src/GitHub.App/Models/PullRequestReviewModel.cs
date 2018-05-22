using System;
using System.Collections.Generic;
using GitHub.Extensions;

namespace GitHub.Models
{
    public class PullRequestReviewModel : IPullRequestReviewModel
    {
        IReadOnlyList<IPullRequestReviewCommentModel> comments;

        public long Id { get; set; }
        public string NodeId { get; set; }
        public IAccount User { get; set; }
        public string Body { get; set; }
        public PullRequestReviewState State { get; set; }
        public string CommitId { get; set; }
        public DateTimeOffset? SubmittedAt { get; set; }

        public IReadOnlyList<IPullRequestReviewCommentModel> Comments
        {
            get { return comments ?? Array.Empty<IPullRequestReviewCommentModel>(); }
            set
            {
                Guard.ArgumentNotNull(value, nameof(value));
                comments = value;
            }
        }
    }
}
