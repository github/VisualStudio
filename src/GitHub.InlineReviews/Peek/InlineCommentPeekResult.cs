using System;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.Extensions;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekResult : IPeekResult
    {
        public InlineCommentPeekResult(InlineCommentThreadViewModel viewModel)
        {
            Guard.ArgumentNotNull(viewModel, nameof(viewModel));

            this.ViewModel = viewModel;
        }

        public bool CanNavigateTo => true;
        public InlineCommentThreadViewModel ViewModel { get; }

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
