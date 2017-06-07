using System.Collections.Generic;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.InlineReviews.ViewModels;
using GitHub.Services;
using Microsoft.VisualStudio.Language.Intellisense;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekableItemSource : IPeekableItemSource
    {
        readonly IApiClientFactory apiClientFactory;
        readonly IPullRequestSessionManager sessionManager;

        public InlineCommentPeekableItemSource(
            IApiClientFactory apiClientFactory,
            IPullRequestSessionManager sessionManager)
        {
            this.apiClientFactory = apiClientFactory;
            this.sessionManager = sessionManager;
        }

        public void AugmentPeekSession(IPeekSession session, IList<IPeekableItem> peekableItems)
        {
            if (session.RelationshipName == InlineCommentPeekRelationship.Instance.Name)
            {
                var viewModel = new InlineCommentPeekViewModel(
                    apiClientFactory,
                    session,
                    sessionManager);
                viewModel.Initialize().Forget();
                peekableItems.Add(new InlineCommentPeekableItem(viewModel));
            }
        }

        public void Dispose()
        {
        }
    }
}
