using System;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.Tags
{
    class ShowInlineCommentTag : InlineCommentTag
    {
        public ShowInlineCommentTag(
            IPullRequestSession session,
            IInlineCommentThreadModel thread)
            : base(session, thread.LineNumber, thread.DiffLineType)
        {
            Guard.ArgumentNotNull(thread, nameof(thread));

            Thread = thread;
        }

        public IInlineCommentThreadModel Thread { get; }
    }
}
