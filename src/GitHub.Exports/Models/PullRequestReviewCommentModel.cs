using System;

namespace GitHub.Models
{
    public class PullRequestReviewCommentModel : CommentModel
    {
        public PullRequestReviewThreadModel Thread { get; set; }
    }
}
