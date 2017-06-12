using System.Collections.Generic;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.ViewModels;
using GitHub.Services;
using Microsoft.VisualStudio.Language.Intellisense;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekableItemSource : IPeekableItemSource
    {
        readonly IApiClientFactory apiClientFactory;
        readonly IInlineCommentPeekService peekService;
        readonly IPullRequestSessionManager sessionManager;

        public InlineCommentPeekableItemSource(
            IApiClientFactory apiClientFactory,
            IInlineCommentPeekService peekService,
            IPullRequestSessionManager sessionManager)
        {
            this.apiClientFactory = apiClientFactory;
            this.peekService = peekService;
            this.sessionManager = sessionManager;
        }

        public void AugmentPeekSession(IPeekSession session, IList<IPeekableItem> peekableItems)
        {
            if (session.RelationshipName == InlineCommentPeekRelationship.Instance.Name)
            {
                var viewModel = new InlineCommentPeekViewModel(
                    apiClientFactory,
                    peekService,
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
