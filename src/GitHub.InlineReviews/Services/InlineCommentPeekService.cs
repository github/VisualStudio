using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.InlineReviews.Peek;
using GitHub.InlineReviews.Tags;
using GitHub.Models;
using GitHub.Primitives;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Differencing;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Text.Projection;

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
        public Tuple<int, bool> GetLineNumber(IPeekSession session, ITrackingPoint point)
        {
            var diffModel = (session.TextView as IWpfTextView)?.TextViewModel as IDifferenceTextViewModel;
            var leftBuffer = false;
            ITextSnapshotLine line = null;

            if (diffModel != null)
            {
                if (diffModel.ViewType == DifferenceViewType.InlineView)
                {
                    // If we're displaying a diff in inline mode, then we're in the left buffer if
                    // the point can be mapped down to the left buffer.
                    var snapshotPoint = point.GetPoint(point.TextBuffer.CurrentSnapshot);
                    var mappedPoint = session.TextView.BufferGraph.MapDownToBuffer(
                        snapshotPoint,
                        PointTrackingMode.Negative,
                        diffModel.Viewer.DifferenceBuffer.LeftBuffer,
                        PositionAffinity.Successor);

                    if (mappedPoint != null)
                    {
                        leftBuffer = true;
                        line = mappedPoint.Value.GetContainingLine();
                    }
                }
                else
                {
                    // If we're displaying a diff in any other mode than inline, then we're in the
                    // left buffer if the session's text view is the diff's left view.
                    leftBuffer = session.TextView == diffModel.Viewer.LeftView;
                }
            }

            if (line == null)
            {
                line = point.GetPoint(point.TextBuffer.CurrentSnapshot).GetContainingLine();
            }

            return Tuple.Create(line.LineNumber, leftBuffer);
        }

        /// <inheritdoc/>
        public void Hide(ITextView textView)
        {
            peekBroker.DismissPeekSession(textView);
        }

        /// <inheritdoc/>
        public ITrackingPoint Show(ITextView textView, AddInlineCommentTag tag)
        {
            Guard.ArgumentNotNull(tag, nameof(tag));

            var line = textView.TextSnapshot.GetLineFromLineNumber(tag.LineNumber);
            var trackingPoint = textView.TextSnapshot.CreateTrackingPoint(line.Start.Position, PointTrackingMode.Positive);

            ExpandCollapsedRegions(textView, line.Extent);

            var session = peekBroker.TriggerPeekSession(textView, trackingPoint, InlineCommentPeekRelationship.Instance.Name);
            var item = session.PeekableItems.OfType<InlineCommentPeekableItem>().FirstOrDefault();

            if (item != null)
            {
                var placeholder = item.ViewModel.Thread.Comments.Last();
                placeholder.CancelEdit.Take(1).Subscribe(_ => session.Dismiss());
            }

            return trackingPoint;
        }

        /// <inheritdoc/>
        public ITrackingPoint Show(ITextView textView, ShowInlineCommentTag tag)
        {
            Guard.ArgumentNotNull(textView, nameof(textView));
            Guard.ArgumentNotNull(tag, nameof(tag));

            var projectionBuffer = textView.TextBuffer as IProjectionBuffer;
            var snapshot = textView.TextSnapshot;

            // If we're displaying a comment on a deleted line, then check if we're displaying in a
            // diff view in inline mode. If so, get the line from the left buffer.
            if (tag.DiffChangeType == DiffChangeType.Delete)
            {
                var diffModel = (textView as IWpfTextView)?.TextViewModel as IDifferenceTextViewModel;

                if (diffModel?.ViewType == DifferenceViewType.InlineView)
                {
                    snapshot = diffModel.Viewer.DifferenceBuffer.LeftBuffer.CurrentSnapshot;
                }
            }

            var line = snapshot.GetLineFromLineNumber(tag.LineNumber);
            var trackingPoint = snapshot.CreateTrackingPoint(line.Start.Position, PointTrackingMode.Positive);
            ExpandCollapsedRegions(textView, line.Extent);
            peekBroker.TriggerPeekSession(textView, trackingPoint, InlineCommentPeekRelationship.Instance.Name);
            return trackingPoint;
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
