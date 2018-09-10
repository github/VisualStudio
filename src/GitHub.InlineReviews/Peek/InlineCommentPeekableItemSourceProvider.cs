using System;
using System.ComponentModel.Composition;
using GitHub.Commands;
using GitHub.Factories;
using GitHub.InlineReviews.Commands;
using GitHub.InlineReviews.Services;
using GitHub.Services;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace GitHub.InlineReviews.Peek
{
    [Export(typeof(IPeekableItemSourceProvider))]
    [ContentType("text")]
    [Name("GitHub Inline Comments Peekable Item Source")]
    class InlineCommentPeekableItemSourceProvider : IPeekableItemSourceProvider
    {
        readonly IInlineReviewPeekService peekService;
        readonly IPullRequestSessionManager sessionManager;
        readonly INextInlineReviewCommand _nextReviewCommand;
        readonly IPreviousInlineCommentCommand previousCommentCommand;
        readonly ICommentService commentService;

        [ImportingConstructor]
        public InlineCommentPeekableItemSourceProvider(
            IInlineReviewPeekService peekService,
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

        public IPeekableItemSource TryCreatePeekableItemSource(ITextBuffer textBuffer)
        {
            return new InlineCommentPeekableItemSource(
                peekService,
                sessionManager,
                _nextReviewCommand,
                previousCommentCommand,
                commentService);
        }
    }
}
