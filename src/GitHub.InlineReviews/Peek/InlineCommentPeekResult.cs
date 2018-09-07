using System;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.Extensions;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.Peek
{
    sealed class InlineCommentPeekResult : IPeekResult
    {
        public InlineCommentPeekResult(InlineReviewPeekViewModel viewModel)
        {
            Guard.ArgumentNotNull(viewModel, nameof(viewModel));

            this.ViewModel = viewModel;
        }

        public bool CanNavigateTo => true;
        public InlineReviewPeekViewModel ViewModel { get; }

        public IPeekResultDisplayInfo DisplayInfo { get; }
            = new PeekResultDisplayInfo("Review", null, "GitHub Review", "GitHub Review");

        public Action<IPeekResult, object, object> PostNavigationCallback => null;

        public event EventHandler Disposed;

        public void Dispose()
        {
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public void NavigateTo(object data)
        {
        }
    }
}
