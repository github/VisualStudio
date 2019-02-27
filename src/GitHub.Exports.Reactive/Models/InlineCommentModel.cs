using System;

namespace GitHub.Models
{
    /// <summary>
    /// Relates a <see cref="PullRequestReviewCommentModel"/> to an
    /// <see cref="IInlineCommentThreadModel"/>.
    /// </summary>
    public class InlineCommentModel
    {
        /// <summary>
        /// Gets or sets the thread that the comment appears in.
        /// </summary>
        public IInlineCommentThreadModel Thread { get; set; }

        /// <summary>
        /// Gets or sets the review that the comment appears in.
        /// </summary>
        public PullRequestReviewModel Review { get; set; }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        public PullRequestReviewCommentModel Comment { get; set; }
    }
}
