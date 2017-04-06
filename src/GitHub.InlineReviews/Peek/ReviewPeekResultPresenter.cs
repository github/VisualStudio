using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;

namespace GitHub.InlineReviews.Peek
{
    [Export(typeof(IPeekResultPresenter))]
    [Name("GitHub Review Peek Presenter")]
    class ReviewPeekResultPresenter : IPeekResultPresenter
    {
        public IPeekResultPresentation TryCreatePeekResultPresentation(IPeekResult result)
        {
            var review = result as ReviewPeekResult;

            if (review != null)
            {
                return new ReviewPeekResultPresentation(review);
            }

            return null;
        }
    }
}
