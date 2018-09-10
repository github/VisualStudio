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
        readonly IInlineReviewPeekService peekService;
        readonly IPullRequestSessionManager sessionManager;
        readonly INextInlineCommentCommand _nextCommentCommand;
        readonly IPreviousInlineCommentCommand previousCommentCommand;
        readonly ICommentService commentService;

        public InlineCommentPeekableItemSource(IInlineReviewPeekService peekService,
            IPullRequestSessionManager sessionManager,
            INextInlineCommentCommand nextCommentCommand,
            IPreviousInlineCommentCommand previousCommentCommand,
            ICommentService commentService)
        {
            this.peekService = peekService;
            this.sessionManager = sessionManager;
            this._nextCommentCommand = nextCommentCommand;
            this.previousCommentCommand = previousCommentCommand;
            this.commentService = commentService;
        }

        public void AugmentPeekSession(IPeekSession session, IList<IPeekableItem> peekableItems)
        {
            if (session.RelationshipName == InlineCommentPeekRelationship.Instance.Name)
            {
                var viewModel = new InlineReviewPeekViewModel(
                    peekService,
                    session,
                    sessionManager,
                    _nextCommentCommand,
                    previousCommentCommand,
                    commentService);
                viewModel.Initialize().Forget();
                peekableItems.Add(new InlineCommentPeekableItem(viewModel));
            }
        }

        public void Dispose()
        {
        }
    }
}
