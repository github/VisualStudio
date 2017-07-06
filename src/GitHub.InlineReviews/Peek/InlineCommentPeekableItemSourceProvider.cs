using System;
using System.ComponentModel.Composition;
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
        readonly IApiClientFactory apiClientFactory;
        readonly IInlineCommentPeekService peekService;
        readonly IPullRequestSessionManager sessionManager;
        readonly INextInlineCommentCommand nextCommentCommand;
        readonly IPreviousInlineCommentCommand previousCommentCommand;

        [ImportingConstructor]
        public InlineCommentPeekableItemSourceProvider(
            IApiClientFactory apiClientFactory,
            IInlineCommentPeekService peekService,
            IPullRequestSessionManager sessionManager,
            INextInlineCommentCommand nextCommentCommand,
            IPreviousInlineCommentCommand previousCommentCommand)
        {
            this.apiClientFactory = apiClientFactory;
            this.peekService = peekService;
            this.sessionManager = sessionManager;
            this.nextCommentCommand = nextCommentCommand;
            this.previousCommentCommand = previousCommentCommand;
        }

        public IPeekableItemSource TryCreatePeekableItemSource(ITextBuffer textBuffer)
        {
            return new InlineCommentPeekableItemSource(
                apiClientFactory,
                peekService,
                sessionManager,
                nextCommentCommand,
                previousCommentCommand);
        }
    }
}
