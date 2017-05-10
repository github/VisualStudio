using System;
using System.Collections.Generic;
using System.Linq;
using GitHub.Extensions;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.InlineReviews.Models
{
    class InlineCommentThreadModel : IInlineCommentThreadModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCommentThreadModel"/> class.
        /// </summary>
        /// <param name="diffMatch">
        /// The last five lines of the thread's diff hunk, in reverse order.
        /// </param>
        public InlineCommentThreadModel(IList<DiffLine> diffMatch)
        {
            Guard.ArgumentNotNull(diffMatch, nameof(diffMatch));

            DiffMatch = diffMatch;
        }

        /// <summary>
        /// Gets or sets the comments in the thread.
        /// </summary>
        public IReactiveList<IPullRequestReviewCommentModel> Comments { get; }
            = new ReactiveList<IPullRequestReviewCommentModel>();

        /// <summary>
        /// Gets the type of line in the diff that the thread was left on.
        /// </summary>
        public DiffChangeType DiffLineType => DiffMatch.First().Type;

        /// <summary>
        /// Gets or sets the last five lines of the thread's diff hunk, in reverse order.
        /// </summary>
        public IList<DiffLine> DiffMatch { get; }

        /// <summary>
        /// Gets or sets the line number that the thread starts on.
        /// </summary>
        public int LineNumber { get; set; }
    }
}
