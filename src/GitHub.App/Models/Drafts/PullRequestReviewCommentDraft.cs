using System;
using GitHub.ViewModels;

namespace GitHub.Models.Drafts
{
    /// <summary>
    /// Stores a draft for a <see cref="PullRequestReviewCommentViewModel"/>
    /// </summary>
    public class PullRequestReviewCommentDraft : CommentDraft
    {
        /// <summary>
        /// Gets or sets the side of the diff that the draft comment was left on.
        /// </summary>
        public DiffSide Side { get; set; }

        /// <summary>
        /// Gets or sets the time that the draft was last updated.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
