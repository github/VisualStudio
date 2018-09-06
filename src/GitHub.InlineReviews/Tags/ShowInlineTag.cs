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
        /// <param name="lineNumber"></param>
        /// <param name="diffLineType"></param>
        public ShowInlineTag(IPullRequestSession session, int lineNumber, DiffChangeType diffLineType)
            : base(session, lineNumber, diffLineType)
        {
        }

        /// <summary>
        /// Gets a model holding details of the thread at the tagged line.
        /// </summary>
        public IInlineCommentThreadModel Thread { get; set; }

        /// <summary>
        /// Gets an array of models holding details of the annotations at the tagged line.
        /// </summary>
        public IInlineAnnotationModel[] Annotations { get; set; }
    }
}
