using GitHub.Extensions;
using GitHub.Services;
using GitHub.Models;
using Microsoft.VisualStudio.Text.Tagging;

namespace GitHub.InlineReviews.Tags
{
    /// <summary>
    /// Base class for inline comment tags.
    /// </summary>
    /// <seealso cref="AddInlineCommentTag"/>
    /// <seealso cref="ShowInlineCommentTag"/>
    public abstract class InlineCommentTag : InlineTagBase
    {
        protected InlineCommentTag(
            IPullRequestSession session,
            int lineNumber,
            DiffChangeType diffChangeType): base(session, lineNumber)
        {
            DiffChangeType = diffChangeType;
        }

        public DiffChangeType DiffChangeType { get; }
    }
}
