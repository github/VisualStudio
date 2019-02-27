using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Classification;
using GitHub.Services;
using GitHub.VisualStudio;
using GitHub.InlineReviews.Services;

namespace GitHub.InlineReviews.Margins
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(InlineCommentMargin.MarginName)]
    [Order(After = PredefinedMarginNames.Glyph)]
    [MarginContainer(PredefinedMarginNames.Left)]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class InlineCommentMarginProvider : IWpfTextViewMarginProvider
    {
        readonly Lazy<IEditorFormatMapService> editorFormatMapService;
        readonly Lazy<IViewTagAggregatorFactoryService> tagAggregatorFactory;
        readonly Lazy<IInlineCommentPeekService> peekService;
        readonly Lazy<IPullRequestSessionManager> sessionManager;
        readonly UIContext uiContext;

        [ImportingConstructor]
        public InlineCommentMarginProvider(
            Lazy<IPullRequestSessionManager> sessionManager,
            Lazy<IEditorFormatMapService> editorFormatMapService,
            Lazy<IViewTagAggregatorFactoryService> tagAggregatorFactory,
            Lazy<IInlineCommentPeekService> peekService)
        {
            this.sessionManager = sessionManager;
            this.editorFormatMapService = editorFormatMapService;
            this.tagAggregatorFactory = tagAggregatorFactory;
            this.peekService = peekService;

            uiContext = UIContext.FromUIContextGuid(new Guid(Guids.GitContextPkgString));
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin parent)
        {
            if (!uiContext.IsActive)
            {
                // Only create margin when in the context of a Git repository
                return null;
            }

            return new InlineCommentMargin(
                wpfTextViewHost,
                peekService.Value,
                editorFormatMapService.Value,
                tagAggregatorFactory.Value,
                sessionManager);
        }
    }
}
