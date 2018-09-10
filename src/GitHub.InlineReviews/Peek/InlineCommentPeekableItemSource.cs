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
        readonly INextInlineReviewCommand _nextReviewCommand;
        readonly IPreviousInlineCommentCommand previousCommentCommand;
        readonly ICommentService commentService;

        public InlineCommentPeekableItemSource(IInlineReviewPeekService peekService,
            IPullRequestSessionManager sessionManager,
            INextInlineReviewCommand nextReviewCommand,
            IPreviousInlineCommentCommand previousCommentCommand,
            ICommentService commentService)
        {
            this.peekService = peekService;
            this.sessionManager = sessionManager;
            this._nextReviewCommand = nextReviewCommand;
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
                    _nextReviewCommand,
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
