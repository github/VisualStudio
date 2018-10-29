using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.ViewModels;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Differencing;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Text.Projection;

namespace GitHub.Services
{
    /// <summary>
    /// Shows inline comments in a peek view.
    /// </summary>
    [Export(typeof(IInlineCommentPeekService))]
    class InlineCommentPeekService : IInlineCommentPeekService
    {
        const string relationship = "GitHubCodeReview";
        readonly IOutliningManagerService outliningService;
        readonly IPeekBroker peekBroker;
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        public InlineCommentPeekService(
            IOutliningManagerService outliningManager,
            IPeekBroker peekBroker,
            IUsageTracker usageTracker)
        {
            this.outliningService = outliningManager;
            this.peekBroker = peekBroker;
            this.usageTracker = usageTracker;
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
        public ITrackingPoint Show(ITextView textView, DiffSide side, int lineNumber)
        {
            var lineAndtrackingPoint = GetLineAndTrackingPoint(textView, side, lineNumber);
            var line = lineAndtrackingPoint.Item1;
            var trackingPoint = lineAndtrackingPoint.Item2;
            var options = new PeekSessionCreationOptions(
                textView,
                relationship,
                trackingPoint,
                defaultHeight: 0);

            ExpandCollapsedRegions(textView, line.Extent);

            var session = peekBroker.TriggerPeekSession(options);
            var item = session.PeekableItems.OfType<IClosable>().FirstOrDefault();
            item?.Closed.Take(1).Subscribe(_ => session.Dismiss());
            
            return trackingPoint;
        }

        Tuple<ITextSnapshotLine, ITrackingPoint> GetLineAndTrackingPoint(
            ITextView textView,
            DiffSide side,
            int lineNumber)
        {
            var diffModel = (textView as IWpfTextView)?.TextViewModel as IDifferenceTextViewModel;
            var snapshot = textView.TextSnapshot;

            if (diffModel?.ViewType == DifferenceViewType.InlineView)
            {
                snapshot = side == DiffSide.Left ?
                    diffModel.Viewer.DifferenceBuffer.LeftBuffer.CurrentSnapshot :
                    diffModel.Viewer.DifferenceBuffer.RightBuffer.CurrentSnapshot;
            }

            var line = snapshot.GetLineFromLineNumber(lineNumber);
            var trackingPoint = snapshot.CreateTrackingPoint(line.Start.Position, PointTrackingMode.Positive);

            ExpandCollapsedRegions(textView, line.Extent);
            peekBroker.TriggerPeekSession(textView, trackingPoint, relationship);

            usageTracker.IncrementCounter(x => x.NumberOfPRReviewDiffViewInlineCommentOpen).Forget();

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
