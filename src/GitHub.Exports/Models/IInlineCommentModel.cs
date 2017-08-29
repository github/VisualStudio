using System;

namespace GitHub.Models
{
    /// <summary>
    /// Represents an pull request review comment that can be displayed inline in a code editor.
    /// </summary>
    public interface IInlineCommentModel
    {
        /// <summary>
        /// Gets the 0-based line number of the comment.
        /// </summary>
        int LineNumber { get; }

        /// <summary>
        /// Gets a value indicating whether the model is stale due to a change in the underlying
        /// file.
        /// </summary>
        bool IsStale { get; }

        /// <summary>
        /// Gets the original pull request review comment.
        /// </summary>
        IPullRequestReviewCommentModel Original { get; }
    }
}
