using System;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// View model for an inline comment (aka Pull Request Review Comment).
    /// </summary>
    interface IInlineCommentViewModel : ICommentViewModel
    {
        /// <summary>
        /// Gets the SHA of the commit that the comment was left on.
        /// </summary>
        string CommitSha { get; }

        /// <summary>
        /// Gets the line on the diff between PR.Base and <see cref="CommitSha"/> that
        /// the comment was left on.
        /// </summary>
        int DiffLine { get; }
    }
}
