using System;
using System.Threading;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekableResultSource : IPeekResultSource
    {
        readonly InlineReviewPeekViewModel viewModel;

        public InlineCommentPeekableResultSource(InlineReviewPeekViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public void FindResults(string relationshipName, IPeekResultCollection resultCollection, CancellationToken cancellationToken, IFindPeekResultsCallback callback)
        {
            resultCollection.Add(new InlineCommentPeekResult(viewModel));
        }
    }
}
