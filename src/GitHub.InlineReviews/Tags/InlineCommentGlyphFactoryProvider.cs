using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using GitHub.Factories;

namespace GitHub.InlineReviews.Tags
{
    [Export]
    class InlineCommentGlyphFactoryProvider : IGlyphMouseProcessorProvider
    {
        readonly IApiClientFactory apiClientFactory;
        readonly IPeekBroker peekBroker;
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;

        [ImportingConstructor]
        public InlineCommentGlyphFactoryProvider(
            IApiClientFactory apiClientFactory,
            IPeekBroker peekBroker,
            IViewTagAggregatorFactoryService tagAggregatorFactory)
        {
            this.apiClientFactory = apiClientFactory;
            this.peekBroker = peekBroker;
            this.tagAggregatorFactory = tagAggregatorFactory;
        }

        public IGlyphFactory<InlineCommentTag> GetGlyphFactory(IWpfTextView view, IWpfTextViewMargin margin)
        {
            return new InlineCommentGlyphFactory();
        }

        public IMouseProcessor GetAssociatedMouseProcessor(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin margin)
        {
            return new InlineCommentGlyphMouseProcessor(
                apiClientFactory,
                peekBroker,
                wpfTextViewHost.TextView,
                margin,
                tagAggregatorFactory.CreateTagAggregator<InlineCommentTag>(wpfTextViewHost.TextView));
        }
    }
}
