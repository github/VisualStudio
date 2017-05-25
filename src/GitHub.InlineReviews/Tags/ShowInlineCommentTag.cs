using System;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Tags
{
    class ShowInlineCommentTag : InlineCommentTag
    {
        public ShowInlineCommentTag(
            IPullRequestSession session,
            ITextView textView,
            IInlineCommentThreadModel thread)
            : base(session, textView, thread.LineNumber)
        {
            Guard.ArgumentNotNull(thread, nameof(thread));

            Thread = thread;
        }

        public IInlineCommentThreadModel Thread { get; }
    }
}
