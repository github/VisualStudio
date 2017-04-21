using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.Models;
using GitHub.InlineReviews.Models;

namespace GitHub.InlineReviews.Peek
{
    class ReviewPeekResult : IPeekResult
    {
        public ReviewPeekResult(IList<InlineCommentModel> comments)
        {
            this.Comments = comments;
        }

        public bool CanNavigateTo => true;
        public IList<InlineCommentModel> Comments { get; }

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
