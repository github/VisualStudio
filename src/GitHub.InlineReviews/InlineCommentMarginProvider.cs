using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.InlineReviews.Tags;
using GitHub.InlineReviews.Glyph;
using GitHub.Factories;

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
        readonly IApiClientFactory apiClientFactory;
        readonly IPeekBroker peekBroker;

        [ImportingConstructor]
        public InlineCommentMarginProvider(
            IEditorFormatMapService editorFormatMapService,
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            IApiClientFactory apiClientFactory,
            IPeekBroker peekBroker)
        {
            this.editorFormatMapService = editorFormatMapService;
            this.tagAggregatorFactory = tagAggregatorFactory;
            this.apiClientFactory = apiClientFactory;
            this.peekBroker = peekBroker;
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin parent)
        {
            var glyphFactory = new InlineCommentGlyphFactory();
            var margin = CreateMargin(glyphFactory, wpfTextViewHost, parent);

            var textView = wpfTextViewHost.TextView;
            var tagAggregator = tagAggregatorFactory.CreateTagAggregator<InlineCommentTag>(textView);
            var mouseProcessor = new InlineCommentGlyphMouseProcessor(apiClientFactory, peekBroker, textView, margin, tagAggregator);
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
