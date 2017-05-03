using System;
using GitHub.Services;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Tags
{
    abstract class InlineCommentTag : IGlyphTag
    {
        public InlineCommentTag(IPullRequestReviewSession session)
        {
            Session = session;
        }

        public IPullRequestReviewSession Session { get; }
    }
}
