using GitHub.Extensions;
using GitHub.Services;
using Microsoft.VisualStudio.Text.Tagging;

namespace GitHub.InlineReviews.Tags
{
    /// <summary>
    /// Base class for inline tags.
    /// </summary>
    public abstract class InlineTagBase : ITag
    {
        protected InlineTagBase(
            IPullRequestSession session,
            int lineNumber)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            LineNumber = lineNumber;
            Session = session;
        }

        public int LineNumber { get; }
        public IPullRequestSession Session { get; }
    }
}