using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;

namespace GitHub.InlineReviews.Peek
{
    [Export(typeof(IPeekResultPresenter))]
    [Name("GitHub Inline Comments Peek Presenter")]
    class InlineCommentPeekResultPresenter : IPeekResultPresenter
    {
        public IPeekResultPresentation TryCreatePeekResultPresentation(IPeekResult result)
        {
            var review = result as InlineCommentPeekResult;
            return review != null ?
                new InlineCommentPeekResultPresentation(review.ViewModel) :
                null;
        }
    }
}
