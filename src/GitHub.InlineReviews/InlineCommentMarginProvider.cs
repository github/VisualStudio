using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Classification;
using GitHub.InlineReviews.Tags;
using GitHub.InlineReviews.Glyph;

namespace GitHub.InlineReviews
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(MarginName)]
    [Order(After = PredefinedMarginNames.Glyph)]
    [MarginContainer(PredefinedMarginNames.Left)]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    //[DeferCreation(OptionName = "TextViewHost/GlyphMargin")]
    internal sealed class InlineCommentMarginProvider : IWpfTextViewMarginProvider
    {
        const string MarginName = "InlineComment";
        const string MarginPropertiesName = "Indicator Margin"; // Same background color as Glyph margin 

        IEditorFormatMapService editorFormatMapService;
        IViewTagAggregatorFactoryService tagAggregatorFactory;
        InlineCommentGlyphMouseProcessorProvider inlineCommentGlyphMouseProcessorProvider;

        [ImportingConstructor]
        public InlineCommentMarginProvider(
            IEditorFormatMapService editorFormatMapService,
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            InlineCommentGlyphMouseProcessorProvider inlineCommentGlyphMouseProcessorProvider)
        {
            this.editorFormatMapService = editorFormatMapService;
            this.tagAggregatorFactory = tagAggregatorFactory;
            this.inlineCommentGlyphMouseProcessorProvider = inlineCommentGlyphMouseProcessorProvider;
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin parent)
        {
            var glyphFactory = new InlineCommentGlyphFactory();
            var margin = CreateMargin(glyphFactory, wpfTextViewHost, parent);
            var mouseProcessor = inlineCommentGlyphMouseProcessorProvider.GetAssociatedMouseProcessor(wpfTextViewHost, margin);
            margin.VisualElement.PreviewMouseLeftButtonUp += (s, e) => mouseProcessor.PreprocessMouseLeftButtonUp(e);
            return margin;
        }

        IWpfTextViewMargin CreateMargin<TGlyphTag>(IGlyphFactory<TGlyphTag> glyphFactory,
            IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin parent) where TGlyphTag : ITag
        {
            var tagAggregator = tagAggregatorFactory.CreateTagAggregator<TGlyphTag>(wpfTextViewHost.TextView);
            return new GlyphMargin<TGlyphTag>(wpfTextViewHost, glyphFactory, tagAggregator,
                editorFormatMapService.GetEditorFormatMap(wpfTextViewHost.TextView),
                MarginPropertiesName, MarginName, true, 17.0);
        }
    }
}
