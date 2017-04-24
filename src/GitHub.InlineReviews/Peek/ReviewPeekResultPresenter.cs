using System;
using System.ComponentModel.Composition;
using GitHub.Factories;
using GitHub.Primitives;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;

namespace GitHub.InlineReviews.Peek
{
    [Export(typeof(IPeekResultPresenter))]
    [Name("GitHub Review Peek Presenter")]
    class ReviewPeekResultPresenter : IPeekResultPresenter
    {
        readonly IApiClientFactory apiClientFactory;

        [ImportingConstructor]
        public ReviewPeekResultPresenter(IApiClientFactory apiClientFactory)
        {
            this.apiClientFactory = apiClientFactory;
        }

        public IPeekResultPresentation TryCreatePeekResultPresentation(IPeekResult result)
        {
            var review = result as ReviewPeekResult;

            if (review != null)
            {
                var hostAddress = HostAddress.Create(review.Session.Repository.CloneUrl.Host);
                var apiClient = apiClientFactory.Create(hostAddress);
                return new ReviewPeekResultPresentation(apiClient, review);
            }

            return null;
        }
    }
}
