using System;
using System.Windows.Controls;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Classification;
using GitHub.InlineReviews.Tags;
using GitHub.InlineReviews.Glyph;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.Views;

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

        readonly IEditorFormatMapService editorFormatMapService;
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;
        readonly IInlineCommentPeekService peekService;

        [ImportingConstructor]
        public InlineCommentMarginProvider(
            IEditorFormatMapService editorFormatMapService,
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            IInlineCommentPeekService peekService)
        {
            this.editorFormatMapService = editorFormatMapService;
            this.tagAggregatorFactory = tagAggregatorFactory;
            this.peekService = peekService;
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin parent)
        {
            var textView = wpfTextViewHost.TextView;
            var tagAggregator = tagAggregatorFactory.CreateTagAggregator<InlineCommentTag>(textView);
            var editorFormatMap = editorFormatMapService.GetEditorFormatMap(textView);
            var glyphFactory = new InlineCommentGlyphFactory(peekService, textView, tagAggregator, editorFormatMap);

            Func<Grid> gridFactory = () => new GlyphMarginGrid();
            return CreateMargin(glyphFactory, gridFactory, wpfTextViewHost, parent, editorFormatMap);
        }

        IWpfTextViewMargin CreateMargin<TGlyphTag>(IGlyphFactory<TGlyphTag> glyphFactory, Func<Grid> gridFactory,
            IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin parent, IEditorFormatMap editorFormatMap) where TGlyphTag : ITag
        {
            var tagAggregator = tagAggregatorFactory.CreateTagAggregator<TGlyphTag>(wpfTextViewHost.TextView);
            return new GlyphMargin<TGlyphTag>(wpfTextViewHost, glyphFactory, gridFactory, tagAggregator, editorFormatMap,
                MarginPropertiesName, MarginName, true, 17.0);
        }
    }
}
