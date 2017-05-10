using System;
using System.Collections.Generic;
using ReactiveUI;

namespace GitHub.Models
{
    /// <summary>
    /// Represents a thread of inline comments on a pull request.
    /// </summary>
    public interface IInlineCommentThreadModel
    {
        /// <summary>
        /// Gets the 0-based line number of the comment.
        /// </summary>
        int LineNumber { get; }

        /// <summary>
        /// Gets the original pull request review comment.
        /// </summary>
        IReactiveList<IPullRequestReviewCommentModel> Comments { get; }

        /// <summary>
        /// Gets or sets the last five lines of the thread's diff hunk, in reverse order.
        /// </summary>
        IList<DiffLine> DiffMatch { get; }
    }
}
