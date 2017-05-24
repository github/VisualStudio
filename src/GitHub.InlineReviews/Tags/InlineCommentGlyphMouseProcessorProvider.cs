using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using GitHub.Factories;

namespace GitHub.InlineReviews.Tags
{
    [Export]
    class InlineCommentGlyphMouseProcessorProvider : IGlyphMouseProcessorProvider
    {
        readonly IApiClientFactory apiClientFactory;
        readonly IPeekBroker peekBroker;
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;

        [ImportingConstructor]
        public InlineCommentGlyphMouseProcessorProvider(
            IApiClientFactory apiClientFactory,
            IPeekBroker peekBroker,
            IViewTagAggregatorFactoryService tagAggregatorFactory)
        {
            this.apiClientFactory = apiClientFactory;
            this.peekBroker = peekBroker;
            this.tagAggregatorFactory = tagAggregatorFactory;
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
