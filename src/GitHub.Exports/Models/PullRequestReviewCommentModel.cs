using System;

namespace GitHub.Models
{
    /// <summary>
    /// Holds details about a pull request review comment.
    /// </summary>
    public class PullRequestReviewCommentModel : CommentModel
    {
        /// <summary>
        /// Gets or sets the associated thread that contains the comment.
        /// </summary>
        public PullRequestReviewThreadModel Thread { get; set; }
    }
}
