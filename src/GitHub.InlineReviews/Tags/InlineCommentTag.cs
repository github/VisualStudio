using GitHub.Extensions;
using GitHub.Services;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace GitHub.InlineReviews.Tags
{
    abstract class InlineCommentTag : ITag
    {
        public InlineCommentTag(
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
