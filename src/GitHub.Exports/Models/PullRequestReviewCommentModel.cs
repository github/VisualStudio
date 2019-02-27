using System;

namespace GitHub.Models
{
    /// <summary>
    /// Holds details about a pull request review comment.
    /// </summary>
    public class PullRequestReviewCommentModel : CommentModel
    {
        /// <summary>
        /// Gets the PullRequestId of the comment.
        /// </summary>
        /// <remarks>
        /// The GraphQL Api does not allow for deleting of pull request comments.
        /// REST Api must be used, and PullRequestId is needed to reload the pull request.
        /// This field should be removed with better GraphQL support.
        /// </remarks>
        public int PullRequestId { get; set; }

        /// <summary>
        /// Gets or sets the associated thread that contains the comment.
        /// </summary>
        public PullRequestReviewThreadModel Thread { get; set; }
    }
}
