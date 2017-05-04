using System;
using System.Threading;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekableResultSource : IPeekResultSource
    {
        readonly InlineCommentThreadViewModel viewModel;

        public InlineCommentPeekableResultSource(InlineCommentThreadViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public void FindResults(string relationshipName, IPeekResultCollection resultCollection, CancellationToken cancellationToken, IFindPeekResultsCallback callback)
        {
            resultCollection.Add(new InlineCommentPeekResult(viewModel));
        }
    }
}
