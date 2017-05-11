using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using GitHub.Services;
using GitHub.Factories;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Tagging;

namespace ScratchMargin
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(PullRequestReviewMargin.MarginName)]
    [Order(Before = PredefinedMarginNames.LineNumber)]
    [MarginContainer(PredefinedMarginNames.LeftSelection)]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class PullRequestReviewMarginFactory : IWpfTextViewMarginProvider
    {
        Lazy<IPullRequestSessionManager> sessionManager;
        Lazy<IApiClientFactory> apiClientFactory;
        Lazy<IPeekBroker> peekBroker;
        Lazy<IViewTagAggregatorFactoryService> tagAggregatorFactory;

        [ImportingConstructor]
        public PullRequestReviewMarginFactory(
            Lazy<IPullRequestSessionManager> sessionManager,
            Lazy<IApiClientFactory> apiClientFactory,
            Lazy<IPeekBroker> peekBroker,
            Lazy<IViewTagAggregatorFactoryService> tagAggregatorFactory)

        {
            this.sessionManager = sessionManager;
            this.apiClientFactory = apiClientFactory;
            this.peekBroker = peekBroker;
            this.tagAggregatorFactory = tagAggregatorFactory;
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            return new PullRequestReviewMargin(wpfTextViewHost, sessionManager.Value, apiClientFactory.Value, peekBroker.Value, tagAggregatorFactory.Value);
        }
    }
}
