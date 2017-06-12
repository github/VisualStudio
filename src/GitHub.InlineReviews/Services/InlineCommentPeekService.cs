using System;
using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.InlineReviews.Peek;
using GitHub.InlineReviews.Tags;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.Primitives;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;

namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// Shows inline comments in a peek view.
    /// </summary>
    [Export(typeof(IInlineCommentPeekService))]
    class InlineCommentPeekService : IInlineCommentPeekService
    {
        readonly IApiClientFactory apiClientFactory;
        readonly IOutliningManagerService outliningService;
        readonly IPeekBroker peekBroker;

        [ImportingConstructor]
        public InlineCommentPeekService(
            IApiClientFactory apiClientFactory,
            IOutliningManagerService outliningManager,
            IPeekBroker peekBroker)
        {
            this.apiClientFactory = apiClientFactory;
            this.outliningService = outliningManager;
            this.peekBroker = peekBroker;
        }

        /// <inheritdoc/>
        public int GetLineNumber(IPeekSession session, ITrackingPoint point)
        {
            return point.GetPoint(session.TextView.TextSnapshot).GetContainingLine().LineNumber;
        }

        /// <inheritdoc/>
        public void Hide(ITextView textView)
        {
            peekBroker.DismissPeekSession(textView);
        }

        /// <inheritdoc/>
        public void Show(ITextView textView, AddInlineCommentTag tag, bool moveCaret = false)
        {
            Guard.ArgumentNotNull(tag, nameof(tag));

            var line = textView.TextSnapshot.GetLineFromLineNumber(tag.LineNumber);
            var trackingPoint = textView.TextSnapshot.CreateTrackingPoint(line.Start.Position, PointTrackingMode.Positive);

            ExpandCollapsedRegions(textView, line.Extent);
            if (moveCaret) textView.Caret.MoveTo(line.Start);

            peekBroker.TriggerPeekSession(textView, trackingPoint, InlineCommentPeekRelationship.Instance.Name);
        }

        /// <inheritdoc/>
        public void Show(ITextView textView, ShowInlineCommentTag tag, bool moveCaret = false)
        {
            Guard.ArgumentNotNull(tag, nameof(tag));

            var line = textView.TextSnapshot.GetLineFromLineNumber(tag.LineNumber);
            var trackingPoint = textView.TextSnapshot.CreateTrackingPoint(line.Start.Position, PointTrackingMode.Positive);

            ExpandCollapsedRegions(textView, line.Extent);
            if (moveCaret) textView.Caret.MoveTo(line.Start);

            peekBroker.TriggerPeekSession(textView, trackingPoint, InlineCommentPeekRelationship.Instance.Name);
        }

        IApiClient CreateApiClient(ILocalRepositoryModel repository)
        {
            var hostAddress = HostAddress.Create(repository.CloneUrl.Host);
            return apiClientFactory.Create(hostAddress);
        }

        void ExpandCollapsedRegions(ITextView textView, SnapshotSpan span)
        {
            var outlining = outliningService.GetOutliningManager(textView);

            if (outlining != null)
            {
                foreach (var collapsed in outlining.GetCollapsedRegions(span))
                {
                    outlining.Expand(collapsed);
                }
            }
        }
    }
}
