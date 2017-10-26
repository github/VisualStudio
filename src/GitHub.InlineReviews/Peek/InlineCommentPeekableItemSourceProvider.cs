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
        readonly IGitHubServiceProvider serviceProvider;
        readonly Lazy<IApiClientFactory> apiClientFactory;
        readonly Lazy<IInlineCommentPeekService> peekService;
        readonly Lazy<IPullRequestSessionManager> sessionManager;
        readonly Lazy<INextInlineCommentCommand> nextCommentCommand;
        readonly Lazy<IPreviousInlineCommentCommand> previousCommentCommand;

        [ImportingConstructor]
        public InlineCommentPeekableItemSourceProvider(
            IGitHubServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.apiClientFactory = new Lazy<IApiClientFactory>(() => serviceProvider.TryGetService<IApiClientFactory>());
            this.peekService = new Lazy<IInlineCommentPeekService>(() => serviceProvider.TryGetService<IInlineCommentPeekService>());
            this.sessionManager = new Lazy<IPullRequestSessionManager>(() => serviceProvider.TryGetService<IPullRequestSessionManager>());
            this.nextCommentCommand = new Lazy<INextInlineCommentCommand>(() => serviceProvider.TryGetService<INextInlineCommentCommand>());
            this.previousCommentCommand = new Lazy<IPreviousInlineCommentCommand>(() => serviceProvider.TryGetService<IPreviousInlineCommentCommand>());
        }

        public IPeekableItemSource TryCreatePeekableItemSource(ITextBuffer textBuffer)
        {
            return new InlineCommentPeekableItemSource(
                apiClientFactory.Value,
                peekService.Value,
                sessionManager.Value,
                nextCommentCommand.Value,
                previousCommentCommand.Value);
        }
    }
}
