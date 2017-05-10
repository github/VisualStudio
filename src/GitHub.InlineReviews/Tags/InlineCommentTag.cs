using System;
using GitHub.Extensions;
using GitHub.Services;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Tags
{
    abstract class InlineCommentTag : IGlyphTag
    {
        public InlineCommentTag(IPullRequestSession session)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            Session = session;
        }

        public IPullRequestSession Session { get; }
    }
}
