using System;
using System.Collections.Generic;
using System.Linq;
using GitHub.Extensions;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.InlineReviews.Models
{
    /// <summary>
    /// Represents a thread of inline comments on an <see cref="IPullRequestSessionFile"/>.
    /// </summary>
    class InlineCommentThreadModel : ReactiveObject, IInlineCommentThreadModel
    {
        bool isStale;
        int lineNumber = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCommentThreadModel"/> class.
        /// </summary>
        /// <param name="relativePath">The relative path to the file that the thread is on.</param>
        /// <param name="commitSha">The SHA of the commit that the thread appears on.</param>
        /// <param name="diffMatch">
        /// The last five lines of the thread's diff hunk, in reverse order.
        /// </param>
        /// <param name="comments">The comments in the thread</param>
        public InlineCommentThreadModel(
            string relativePath,
            string commitSha,
            IList<DiffLine> diffMatch,
            IEnumerable<InlineCommentModel> comments)
        {
            Guard.ArgumentNotNull(relativePath, nameof(relativePath));
            Guard.ArgumentNotNull(commitSha, nameof(commitSha));
            Guard.ArgumentNotNull(diffMatch, nameof(diffMatch));

            Comments = comments.ToList();
            DiffMatch = diffMatch;
            DiffLineType = diffMatch[0].Type;
            CommitSha = commitSha;
            RelativePath = relativePath;

            foreach (var comment in comments)
            {
                comment.Thread = this;
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<InlineCommentModel> Comments { get; }

        /// <inheritdoc/>
        public IList<DiffLine> DiffMatch { get; }

        /// <inheritdoc/>
        public DiffChangeType DiffLineType { get; }

        /// <inheritdoc/>
        public bool IsStale
        {
            get { return isStale; }
            set { this.RaiseAndSetIfChanged(ref isStale, value); }
        }

        /// <inheritdoc/>
        public int LineNumber
        {
            get { return lineNumber; }
            set { this.RaiseAndSetIfChanged(ref lineNumber, value); }
        }

        /// <inheritdoc/>
        public string CommitSha { get; }

        /// <inheritdoc/>
        public string RelativePath { get; }
    }
}
