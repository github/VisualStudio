using System;
using System.ComponentModel.Composition;
using GitHub.Factories;
using GitHub.Primitives;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;

namespace GitHub.InlineReviews.Peek
{
    [Export(typeof(IPeekResultPresenter))]
    [Name("GitHub Inline Comments Peek Presenter")]
    class InlineCommentPeekResultPresenter : IPeekResultPresenter
    {
        readonly IApiClientFactory apiClientFactory;

        [ImportingConstructor]
        public InlineCommentPeekResultPresenter(IApiClientFactory apiClientFactory)
        {
            this.apiClientFactory = apiClientFactory;
        }

        public IPeekResultPresentation TryCreatePeekResultPresentation(IPeekResult result)
        {
            var review = result as InlineCommentPeekResult;

            if (review != null)
            {
                var hostAddress = HostAddress.Create(review.Session.Repository.CloneUrl.Host);
                var apiClient = apiClientFactory.Create(hostAddress);
                return new InlineCommentPeekResultPresentation(apiClient, review);
            }

            return null;
        }
    }
}
