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
        readonly IInlineCommentPeekService peekService;
        readonly IPullRequestSessionManager sessionManager;
        readonly INextInlineCommentCommand _nextCommentCommand;
        readonly IPreviousInlineCommentCommand previousCommentCommand;
        readonly ICommentService commentService;

        [ImportingConstructor]
        public InlineCommentPeekableItemSourceProvider(
            IInlineCommentPeekService peekService,
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

        public IPeekableItemSource TryCreatePeekableItemSource(ITextBuffer textBuffer)
        {
            return new InlineCommentPeekableItemSource(
                peekService,
                sessionManager,
                _nextCommentCommand,
                previousCommentCommand,
                commentService);
        }
    }
}
