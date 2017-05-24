using GitHub.Extensions;
using GitHub.Services;
using Microsoft.VisualStudio.Text.Tagging;

namespace GitHub.InlineReviews.Tags
{
    abstract class InlineCommentTag : ITag
    {
        public InlineCommentTag(IPullRequestSession session)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            Session = session;
        }

        public IPullRequestSession Session { get; }
    }
}
