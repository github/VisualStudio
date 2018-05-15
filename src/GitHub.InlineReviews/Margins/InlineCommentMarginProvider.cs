using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Classification;
using GitHub.InlineReviews.Services;
using GitHub.Services;
using GitHub.Settings;

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
        readonly IEditorFormatMapService editorFormatMapService;
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;
        readonly IInlineCommentPeekService peekService;
        readonly Lazy<IPullRequestSessionManager> sessionManager;
        readonly InlineCommentMarginEnabled inlineCommentMarginEnabled;

        [ImportingConstructor]
        public InlineCommentMarginProvider(
            Lazy<IPullRequestSessionManager> sessionManager,
            IEditorFormatMapService editorFormatMapService,
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            IInlineCommentPeekService peekService,
            InlineCommentMarginEnabled inlineCommentMarginEnabled)
        {
            this.editorFormatMapService = editorFormatMapService;
            this.tagAggregatorFactory = tagAggregatorFactory;
            this.peekService = peekService;
            this.sessionManager = sessionManager;
            this.inlineCommentMarginEnabled = inlineCommentMarginEnabled;
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin parent)
        {
            return new InlineCommentMargin(
                wpfTextViewHost, peekService, editorFormatMapService, tagAggregatorFactory, sessionManager,
                inlineCommentMarginEnabled);
        }
    }
}
