using System;
using GitHub.Extensions;
using GitHub.InlineReviews.Models;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.Tags
{
    class ShowInlineCommentTag : InlineCommentTag
    {
        public ShowInlineCommentTag(IPullRequestSession session, IInlineCommentThreadModel thread)
            : base(session)
        {
            Guard.ArgumentNotNull(thread, nameof(thread));

            Thread = thread;
        }

        public IInlineCommentThreadModel Thread { get; }
        public bool IsAddNewCommentTag => Thread.Comments.Count == 0;
        ////public bool NeedsUpdate => Thread.Any(x => x.IsStale);
    }
}
