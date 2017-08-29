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
        int lineNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCommentThreadModel"/> class.
        /// </summary>
        /// <param name="relativePath">The relative path to the file that the thread is on.</param>
        /// <param name="originalCommitSha">The SHA of the commit that the thread was left con.</param>
        /// <param name="originalPosition">
        /// The 1-based line number in the original diff that the thread was left on.
        /// </param>
        /// <param name="diffMatch">
        /// The last five lines of the thread's diff hunk, in reverse order.
        /// </param>
        public InlineCommentThreadModel(
            string relativePath,
            string originalCommitSha,
            int originalPosition,
            IList<DiffLine> diffMatch,
            IEnumerable<IPullRequestReviewCommentModel> comments)
        {
            Guard.ArgumentNotNull(originalCommitSha, nameof(originalCommitSha));
            Guard.ArgumentNotNull(diffMatch, nameof(diffMatch));

            Comments = comments.ToList();
            DiffMatch = diffMatch;
            OriginalCommitSha = originalCommitSha;
            OriginalPosition = originalPosition;
            RelativePath = relativePath;
        }

        /// <inheritdoc/>
        public IReadOnlyList<IPullRequestReviewCommentModel> Comments { get; }

        /// <inheritdoc/>
        public IList<DiffLine> DiffMatch { get; }

        /// <inheritdoc/>
        public DiffChangeType DiffLineType => DiffMatch.First().Type;

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
        public string OriginalCommitSha { get; }

        /// <inheritdoc/>
        public int OriginalPosition { get; }

        /// <inheritdoc/>
        public string RelativePath { get; }
    }
}
