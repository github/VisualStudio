using System;

namespace GitHub.Models
{
    public class InlineCommentModel
    {
        public IInlineCommentThreadModel Thread { get; set; }
        public PullRequestReviewModel Review { get; set; }
        public PullRequestReviewCommentModel Comment { get; set; }
    }
}
