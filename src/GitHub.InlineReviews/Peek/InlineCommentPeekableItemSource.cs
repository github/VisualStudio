using System.Collections.Generic;
using GitHub.Commands;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.InlineReviews.Commands;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.ViewModels;
using GitHub.Services;
using Microsoft.VisualStudio.Language.Intellisense;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekableItemSource : IPeekableItemSource
    {
        readonly IInlineCommentPeekService peekService;
        readonly IPullRequestSessionManager sessionManager;
        readonly INextInlineCommentCommand nextCommentCommand;
        readonly IPreviousInlineCommentCommand previousCommentCommand;
        readonly IViewViewModelFactory factory;

        public InlineCommentPeekableItemSource(IInlineCommentPeekService peekService,
            IPullRequestSessionManager sessionManager,
            INextInlineCommentCommand nextCommentCommand,
            IPreviousInlineCommentCommand previousCommentCommand,
            IViewViewModelFactory factory)
        {
            this.peekService = peekService;
            this.sessionManager = sessionManager;
            this.nextCommentCommand = nextCommentCommand;
            this.previousCommentCommand = previousCommentCommand;
            this.factory = factory;
        }

        public void AugmentPeekSession(IPeekSession session, IList<IPeekableItem> peekableItems)
        {
            if (session.RelationshipName == InlineCommentPeekRelationship.Instance.Name)
            {
                var viewModel = new InlineCommentPeekViewModel(
                    peekService,
                    session,
                    sessionManager,
                    nextCommentCommand,
                    previousCommentCommand,
                    factory);
                viewModel.Initialize().Forget();
                peekableItems.Add(new InlineCommentPeekableItem(viewModel));
            }
        }

        public void Dispose()
        {
        }
    }
}
