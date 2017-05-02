using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace GitHub.InlineReviews.Tags
{
    [Export(typeof(IGlyphFactoryProvider))]
    [Export(typeof(IGlyphMouseProcessorProvider))]
    [Name("ReviewGlyph")]
    [Order(Before = "VsTextMarker")]
    [ContentType("code")]
    [TagType(typeof(InlineCommentTag))]
    class InlineCommentGlyphFactoryProvider : IGlyphFactoryProvider, IGlyphMouseProcessorProvider
    {
        readonly IPeekBroker peekBroker;
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;

        [ImportingConstructor]
        public InlineCommentGlyphFactoryProvider(
            IPeekBroker peekBroker,
            IViewTagAggregatorFactoryService tagAggregatorFactory)
        {
            this.peekBroker = peekBroker;
            this.tagAggregatorFactory = tagAggregatorFactory;
        }

        public IGlyphFactory GetGlyphFactory(IWpfTextView view, IWpfTextViewMargin margin)
        {
            return new InlineCommentGlyphFactory();
        }

        public IMouseProcessor GetAssociatedMouseProcessor(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin margin)
        {
            return new InlineCommentGlyphMouseProcessor(
                peekBroker,
                wpfTextViewHost.TextView,
                margin,
                tagAggregatorFactory.CreateTagAggregator<InlineCommentTag>(wpfTextViewHost.TextView));
        }
    }
}
