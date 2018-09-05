using System;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.Tags
{
    /// <summary>
    /// A tag which marks a line where inline review comments are present.
    /// </summary>
    public class ShowInlineTag : InlineTag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShowInlineTag"/> class.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="thread">A model holding the details of the thread.</param>
        /// <param name="lineNumber"></param>
        /// <param name="diffLineType"></param>
        public ShowInlineTag(IPullRequestSession session,
            IInlineCommentThreadModel thread, int lineNumber, DiffChangeType diffLineType)
            : base(session, lineNumber, diffLineType)
        {
            Thread = thread;
        }

        /// <summary>
        /// Gets a model holding details of the thread at the tagged line.
        /// </summary>
        public IInlineCommentThreadModel Thread { get; }
    }
}
