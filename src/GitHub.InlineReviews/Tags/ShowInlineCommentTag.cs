using System;
using System.Collections.Generic;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.Tags
{
    /// <summary>
    /// A tag which marks a line where inline review comments are present.
    /// </summary>
    public class ShowInlineCommentTag : InlineCommentTag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShowInlineCommentTag"/> class.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="lineNumber"></param>
        /// <param name="diffLineType"></param>
        public ShowInlineCommentTag(IPullRequestSession session, int lineNumber, DiffChangeType diffLineType)
            : base(session, lineNumber, diffLineType)
        {
        }

        /// <summary>
        /// Gets a model holding details of the thread at the tagged line.
        /// </summary>
        public IInlineCommentThreadModel Thread { get; set; }

        /// <summary>
        /// Gets a list of models holding details of the annotations at the tagged line.
        /// </summary>
        public IReadOnlyList<InlineAnnotationModel> Annotations { get; set; }
    }
}
