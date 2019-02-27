using System.ComponentModel.Composition;
using GitHub.Commands;
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
        readonly IInlineCommentPeekService peekService;
        readonly IPullRequestSessionManager sessionManager;
        readonly INextInlineCommentCommand nextCommentCommand;
        readonly IPreviousInlineCommentCommand previousCommentCommand;
        readonly IViewViewModelFactory factory;

        [ImportingConstructor]
        public InlineCommentPeekableItemSourceProvider(
            IInlineCommentPeekService peekService,
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

        public IPeekableItemSource TryCreatePeekableItemSource(ITextBuffer textBuffer)
        {
            return new InlineCommentPeekableItemSource(
                peekService,
                sessionManager,
                nextCommentCommand,
                previousCommentCommand,
                factory);
        }
    }
}
