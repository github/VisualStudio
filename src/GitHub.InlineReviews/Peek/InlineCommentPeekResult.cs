using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.InlineReviews.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekResult : IPeekResult
    {
        public InlineCommentPeekResult(
            IPullRequestReviewSession session,
            IList<InlineCommentModel> comments)
        {
            this.Session = session;
            this.Comments = comments;
        }

        public bool CanNavigateTo => true;
        public IList<InlineCommentModel> Comments { get; }
        public IPullRequestReviewSession Session { get; }

        public IPeekResultDisplayInfo DisplayInfo { get; }
            = new PeekResultDisplayInfo("Review", null, "GitHub Review", "GitHub Review");

        public Action<IPeekResult, object, object> PostNavigationCallback => null;

        public event EventHandler Disposed;

        public void Dispose()
        {
        }

        public void NavigateTo(object data)
        {
        }
    }
}
