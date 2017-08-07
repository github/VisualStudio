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
                    // If we're displaying a diff in inline mode, then we need to map the point down
                    // to the left or right buffer.
                    var snapshotPoint = point.GetPoint(point.TextBuffer.CurrentSnapshot);
                    var mappedPoint = session.TextView.BufferGraph.MapDownToFirstMatch(
                        snapshotPoint,
                        PointTrackingMode.Negative,
                        x => !(x is IProjectionSnapshot),
                        PositionAffinity.Successor);

                    if (mappedPoint != null)
                    {
                        leftBuffer = mappedPoint.Value.Snapshot == diffModel.Viewer.DifferenceBuffer.LeftBuffer.CurrentSnapshot;
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

            var lineAndtrackingPoint = GetLineAndTrackingPoint(textView, tag);
            var line = lineAndtrackingPoint.Item1;
            var trackingPoint = lineAndtrackingPoint.Item2;
            var options = new PeekSessionCreationOptions(
                textView,
                InlineCommentPeekRelationship.Instance.Name,
                trackingPoint,
                defaultHeight: 0);

            ExpandCollapsedRegions(textView, line.Extent);

            var session = peekBroker.TriggerPeekSession(options);
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

            var lineAndtrackingPoint = GetLineAndTrackingPoint(textView, tag);
            var line = lineAndtrackingPoint.Item1;
            var trackingPoint = lineAndtrackingPoint.Item2;
            var options = new PeekSessionCreationOptions(
                textView,
                InlineCommentPeekRelationship.Instance.Name,
                trackingPoint,
                defaultHeight: 0);

            ExpandCollapsedRegions(textView, line.Extent);

            peekBroker.TriggerPeekSession(options);

            return trackingPoint;
        }

        Tuple<ITextSnapshotLine, ITrackingPoint> GetLineAndTrackingPoint(ITextView textView, InlineCommentTag tag)
        {
            var diffModel = (textView as IWpfTextView)?.TextViewModel as IDifferenceTextViewModel;
            var snapshot = textView.TextSnapshot;

            if (diffModel?.ViewType == DifferenceViewType.InlineView)
            {
                snapshot = tag.DiffChangeType == DiffChangeType.Delete ?
                    diffModel.Viewer.DifferenceBuffer.LeftBuffer.CurrentSnapshot :
                    diffModel.Viewer.DifferenceBuffer.RightBuffer.CurrentSnapshot;
            }

            var line = snapshot.GetLineFromLineNumber(tag.LineNumber);
            var trackingPoint = snapshot.CreateTrackingPoint(line.Start.Position, PointTrackingMode.Positive);
            return Tuple.Create(line, trackingPoint);
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
