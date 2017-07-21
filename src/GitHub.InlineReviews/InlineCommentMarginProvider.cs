using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text;
using GitHub.InlineReviews.Tags;
using GitHub.InlineReviews.Glyph;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.Views;
using GitHub.Services;

namespace GitHub.InlineReviews
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(MarginName)]
    [Order(After = PredefinedMarginNames.Glyph)]
    [MarginContainer(PredefinedMarginNames.Left)]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class InlineCommentMarginProvider : IWpfTextViewMarginProvider
    {
        const string MarginName = "InlineComment";
        const string MarginPropertiesName = "Indicator Margin"; // Same background color as Glyph margin 

        readonly IEditorFormatMapService editorFormatMapService;
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;
        readonly IInlineCommentPeekService peekService;
        readonly IPullRequestSessionManager sessionManager;

        [ImportingConstructor]
        public InlineCommentMarginProvider(
            IEditorFormatMapService editorFormatMapService,
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            IInlineCommentPeekService peekService,
            IPullRequestSessionManager sessionManager)
        {
            this.editorFormatMapService = editorFormatMapService;
            this.tagAggregatorFactory = tagAggregatorFactory;
            this.peekService = peekService;
            this.sessionManager = sessionManager;
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin parent)
        {
            var textView = wpfTextViewHost.TextView;
            var glyphFactory = new InlineCommentGlyphFactory(peekService, textView);

            Func<Grid> gridFactory = () => new GlyphMarginGrid();
            var editorFormatMap = editorFormatMapService.GetEditorFormatMap(textView);
            return CreateMargin(glyphFactory, gridFactory, wpfTextViewHost, parent, editorFormatMap);
        }

        IWpfTextViewMargin CreateMargin<TGlyphTag>(IGlyphFactory<TGlyphTag> glyphFactory, Func<Grid> gridFactory,
            IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin parent, IEditorFormatMap editorFormatMap) where TGlyphTag : ITag
        {
            var tagAggregator = tagAggregatorFactory.CreateTagAggregator<TGlyphTag>(wpfTextViewHost.TextView);
            var margin = new GlyphMargin<TGlyphTag>(wpfTextViewHost, glyphFactory, gridFactory, tagAggregator, editorFormatMap,
                IsMarginVisible, MarginPropertiesName, MarginName, true, 17.0);

            TrackCommentGlyphOnDiffView(wpfTextViewHost, margin.VisualElement);
            return margin;
        }

        void TrackCommentGlyphOnDiffView(IWpfTextViewHost host, UIElement marginElement)
        {
            var textView = host.TextView;
            if (textView.Roles.Contains("DIFF"))
            {
                var router = new MouseEnterAndLeaveEventRouter<AddInlineCommentGlyph>();
                router.Add(host.HostControl, marginElement);
            }
        }

        bool IsMarginVisible(ITextBuffer buffer)
        {
            if (sessionManager.GetTextBufferInfo(buffer) != null)
            {
                return true;
            }

            InlineCommentTagger inlineCommentTagger;
            if (buffer.Properties.TryGetProperty(typeof(InlineCommentTagger), out inlineCommentTagger))
            {
                if (inlineCommentTagger.ShowMargin)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
