using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    /// <summary>
    /// Represents a thread of inline comments on an <see cref="IPullRequestSessionFile"/>.
    /// </summary>
    public interface IInlineCommentThreadModel
    {
        /// <summary>
        /// Gets or sets the comments in the thread.
        /// </summary>
        IReadOnlyList<IPullRequestReviewCommentModel> Comments { get; }

        /// <summary>
        /// Gets the last five lines of the thread's diff hunk, in reverse order.
        /// </summary>
        IList<DiffLine> DiffMatch { get; }

        /// <summary>
        /// Gets the type of diff line that the thread was left on
        /// </summary>
        DiffChangeType DiffLineType { get; }

        /// <summary>
        /// Gets or sets a value indicating that the <see cref="LineNumber"/> is approximate and
        /// needs to be updated.
        /// </summary>
        /// <remarks>
        /// As edits are made, the <see cref="LineNumber"/> for a thread can be shifted up or down,
        /// but until <see cref="IPullRequestSession.RecaluateLineNumbers"/> is called we can't tell
        /// whether the comment is still valid at the new position. This property indicates such a
        /// state.
        /// </remarks>
        bool IsStale { get; set; }

        /// <summary>
        /// Gets or sets the 0-based line number of the comment, or -1 of the thread is outdated.
        /// </summary>
        int LineNumber { get; set; }

        /// <summary>
        /// Gets the SHA of the commit that the thread appears on.
        /// </summary>
        string CommitSha { get; }

        /// <summary>
        /// Gets the relative path to the file that the thread is on.
        /// </summary>
        string RelativePath { get; }
    }
}
