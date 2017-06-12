using System;
using System.ComponentModel.Composition;
using GitHub.Factories;
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

        [ImportingConstructor]
        public InlineCommentPeekableItemSourceProvider(
            IApiClientFactory apiClientFactory,
            IInlineCommentPeekService peekService,
            IPullRequestSessionManager sessionManager)
        {
            this.apiClientFactory = apiClientFactory;
            this.peekService = peekService;
            this.sessionManager = sessionManager;
        }

        public IPeekableItemSource TryCreatePeekableItemSource(ITextBuffer textBuffer)
        {
            return new InlineCommentPeekableItemSource(apiClientFactory, peekService, sessionManager);
        }
    }
}
