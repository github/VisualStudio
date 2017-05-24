using System;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Classification;
using GitHub.InlineReviews.Tags;

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
        InlineCommentGlyphFactoryProvider inlineCommentGlyphFactoryProvider;

        [ImportingConstructor]
        public InlineCommentMarginProvider(
            IEditorFormatMapService editorFormatMapService,
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            InlineCommentGlyphFactoryProvider inlineCommentGlyphFactoryProvider)
        {
            this.editorFormatMapService = editorFormatMapService;
            this.tagAggregatorFactory = tagAggregatorFactory;
            this.inlineCommentGlyphFactoryProvider = inlineCommentGlyphFactoryProvider;
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin parent)
        {
            var glyphFactory = inlineCommentGlyphFactoryProvider.GetGlyphFactory(wpfTextViewHost.TextView, parent); // HACK: Using parent margin.
            var glyphFactories = new InlineCommentGlyphFactories(glyphFactory);
            var margin = CreateMargin(glyphFactories, wpfTextViewHost, parent);
            var mouseProcessor = inlineCommentGlyphFactoryProvider.GetAssociatedMouseProcessor(wpfTextViewHost, margin);
            margin.VisualElement.PreviewMouseLeftButtonUp += (s, e) => mouseProcessor.PreprocessMouseLeftButtonUp(e);
            return margin;
        }

        IWpfTextViewMargin CreateMargin<TGlyphTag>(IGlyphFactories<TGlyphTag> glyphFactories,
            IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin parent) where TGlyphTag : ITag
        {
            var tagAggregator = tagAggregatorFactory.CreateTagAggregator<TGlyphTag>(wpfTextViewHost.TextView);
            return new GlyphMargin<TGlyphTag>(wpfTextViewHost, glyphFactories, tagAggregator,
                editorFormatMapService.GetEditorFormatMap(wpfTextViewHost.TextView),
                MarginPropertiesName, MarginName, true, 17.0);
        }
    }

    internal class InlineCommentGlyphFactories : IGlyphFactories<InlineCommentTag>, IGlyphFactory<InlineCommentTag>
    {
        IGlyphFactory glyphFactory;

        internal InlineCommentGlyphFactories(IGlyphFactory glyphFactory)
        {
            this.glyphFactory = glyphFactory;
        }

        public IEnumerable<Type> GetTagTypes()
        {
            return new[] { typeof(ShowInlineCommentTag), typeof(AddInlineCommentTag) };
        }

        public IGlyphFactory<InlineCommentTag> InstantiateGlyphFactory(Type glyphType, IWpfTextView textView, IWpfTextViewMargin margin)
        {
            return this;
        }

        public UIElement GenerateGlyph(IWpfTextViewLine line, InlineCommentTag tag)
        {
            return glyphFactory.GenerateGlyph(line, tag);
        }
    }
}

