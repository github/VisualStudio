using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GitHub.Commands;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.Tags;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using GitHub.Services.Vssdk.Commands;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Differencing;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.TextManager.Interop;
using Serilog;

namespace GitHub.InlineReviews.Commands
{
    /// <summary>
    /// Base class for commands that navigate between inline comments.
    /// </summary>
    abstract class InlineCommentNavigationCommand : VsCommand<InlineCommentNavigationParams>
    {
        static readonly ILogger log = LogManager.ForContext<InlineCommentNavigationCommand>();
        readonly IGitHubServiceProvider serviceProvider;
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;
        readonly IInlineCommentPeekService peekService;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCommentNavigationCommand"/> class.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="tagAggregatorFactory">The tag aggregator factory.</param>
        /// <param name="peekService">The peek service.</param>
        /// <param name="commandSet">The GUID of the group the command belongs to.</param>
        /// <param name="commandId">The numeric identifier of the command.</param>
        protected InlineCommentNavigationCommand(
            IGitHubServiceProvider serviceProvider,
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            IInlineCommentPeekService peekService,
            Guid commandSet,
            int commandId)
            : base(commandSet, commandId)
        {
            this.serviceProvider = serviceProvider;
            this.tagAggregatorFactory = tagAggregatorFactory;
            this.peekService = peekService;
            BeforeQueryStatus += QueryStatus;
        }

        /// <summary>
        /// Gets the text buffer position for the line specified in the parameters or from the
        /// cursor point if no line is specified or <paramref name="parameter"/> is null.
        /// </summary>
        /// <param name="parameter">The parameters.</param>
        /// <param name="textView">The text view.</param>
        /// <returns></returns>
        protected int GetCursorPoint(ITextView textView, InlineCommentNavigationParams parameter)
        {
            if (parameter?.FromLine != null)
            {
                return parameter.FromLine > -1 ? GetCursorPoint(textView, parameter.FromLine.Value) : -1;
            }
            else
            {
                return textView.Caret.Position.BufferPosition.Position;
            }
        }

        /// <summary>
        /// Gets the text buffer position for the specified line.
        /// </summary>
        /// <param name="textView">The text view containing the buffer</param>
        /// <param name="lineNumber">The 0-based line number.</param>
        /// <returns></returns>
        protected static int GetCursorPoint(ITextView textView, int lineNumber)
        {
            lineNumber = Math.Max(0, Math.Min(lineNumber, textView.TextSnapshot.LineCount - 1));
            return textView.TextSnapshot.GetLineFromLineNumber(lineNumber).Start.Position;
        }

        /// <summary>
        /// Gets the currently active text view(s) from Visual Studio.
        /// </summary>
        /// <returns>
        /// Zero, one or two active <see cref="ITextView"/> objects.
        /// </returns>
        /// <remarks>
        /// This method will return a single text view for a normal code window, or a pair of text
        /// views if the currently active text view is a difference view in side by side mode, with
        /// the first item being the side that currently has focus. If there is no active text view,
        /// an empty collection will be returned.
        /// </remarks>
        protected IEnumerable<ITextView> GetCurrentTextViews()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var result = new List<ITextView>();

            try
            {
                var monitorSelection = (IVsMonitorSelection)serviceProvider.GetService(typeof(SVsShellMonitorSelection));
                if (monitorSelection == null)
                {
                    return result;
                }

                object curDocument;
                if (ErrorHandler.Failed(monitorSelection.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, out curDocument)))
                {
                    return result;
                }

                IVsWindowFrame frame = curDocument as IVsWindowFrame;
                if (frame == null)
                {
                    return result;
                }

                object docView = null;
                if (ErrorHandler.Failed(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView)))
                {
                    return result;
                }

                if (docView is IVsDifferenceCodeWindow)
                {
                    var diffWindow = (IVsDifferenceCodeWindow)docView;

                    switch (diffWindow.DifferenceViewer.ViewMode)
                    {
                        case DifferenceViewMode.Inline:
                            result.Add(diffWindow.DifferenceViewer.InlineView);
                            break;
                        case DifferenceViewMode.SideBySide:
                            switch (diffWindow.DifferenceViewer.ActiveViewType)
                            {
                                case DifferenceViewType.LeftView:
                                    result.Add(diffWindow.DifferenceViewer.LeftView);
                                    result.Add(diffWindow.DifferenceViewer.RightView);
                                    break;
                                case DifferenceViewType.RightView:
                                    result.Add(diffWindow.DifferenceViewer.RightView);
                                    result.Add(diffWindow.DifferenceViewer.LeftView);
                                    break;
                            }
                            result.Add(diffWindow.DifferenceViewer.LeftView);
                            break;
                        case DifferenceViewMode.RightViewOnly:
                            result.Add(diffWindow.DifferenceViewer.RightView);
                            break;
                    }
                }
                else if (docView is IVsCodeWindow)
                {
                    IVsTextView textView;
                    if (ErrorHandler.Failed(((IVsCodeWindow)docView).GetPrimaryView(out textView)))
                    {
                        return result;
                    }

                    var model = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
                    Assumes.Present(model);

                    var adapterFactory = model.GetService<IVsEditorAdaptersFactoryService>();
                    var wpfTextView = adapterFactory.GetWpfTextView(textView);
                    result.Add(wpfTextView);
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Exception in InlineCommentNavigationCommand.GetCurrentTextViews()");
            }

            return result;
        }

        /// <summary>
        /// Creates a tag aggregator for the specified text view.
        /// </summary>
        /// <param name="textView">The text view.</param>
        /// <returns>The tag aggregator</returns>
        protected ITagAggregator<InlineCommentTag> CreateTagAggregator(ITextView textView)
        {
            return tagAggregatorFactory.CreateTagAggregator<InlineCommentTag>(textView);
        }

        /// <summary>
        /// Gets the <see cref="ShowInlineCommentTag"/>s for the specified text view.
        /// </summary>
        /// <param name="textViews">The active text views.</param>
        /// <returns>A collection of <see cref="ITagInfo"/> objects, ordered by line.</returns>
        protected IReadOnlyList<ITagInfo> GetTags(IEnumerable<ITextView> textViews)
        {
            var result = new List<ITagInfo>();

            foreach (var textView in textViews)
            {
                var tagAggregator = CreateTagAggregator(textView);
                var span = new SnapshotSpan(textView.TextSnapshot, 0, textView.TextSnapshot.Length);
                var mappingSpan = textView.BufferGraph.CreateMappingSpan(span, SpanTrackingMode.EdgeExclusive);
                var tags = tagAggregator.GetTags(mappingSpan)
                    .Select(x => new TagInfo
                    {
                        TextView = textView,
                        Point = Map(x.Span.Start, textView.TextSnapshot),
                        Tag = x.Tag as ShowInlineCommentTag,
                    })
                    .Where(x => x.Tag != null && x.Point.HasValue);
                result.AddRange(tags);
            }

            result.Sort(TagInfoComparer.Instance);
            return result;
        }

        /// <summary>
        /// Shows the inline comments for the specified tag in a peek view.
        /// </summary>
        /// <param name="textView">The text view containing the tag</param>
        /// <param name="tag">The inline comment tag</param>
        /// <param name="parameter">The navigation parameter detailing a search from the specified tag</param>
        /// <param name="allTextViews">The full list of text views</param>
        protected void ShowPeekComments(
            InlineCommentNavigationParams parameter,
            ITextView textView,
            ShowInlineCommentTag tag,
            IEnumerable<ITextView> allTextViews)
        {
            foreach (var other in allTextViews)
            {
                if (other != textView)
                {
                    peekService.Hide(other);
                }
            }

            var side = tag.DiffChangeType == DiffChangeType.Delete ? DiffSide.Left : DiffSide.Right;
            var point = peekService.Show(textView, side, tag.LineNumber);

            if (parameter?.MoveCursor != false)
            {
                var caretPoint = textView.BufferGraph.MapUpToSnapshot(
                    point.GetPoint(point.TextBuffer.CurrentSnapshot),
                    PointTrackingMode.Negative,
                    PositionAffinity.Successor,
                    textView.TextSnapshot);

                if (caretPoint.HasValue)
                {
                    (textView as FrameworkElement)?.Focus();
                    textView.Caret.MoveTo(caretPoint.Value);
                }
            }
        }

        static SnapshotPoint? Map(IMappingPoint p, ITextSnapshot textSnapshot)
        {
            return p.GetPoint(textSnapshot.TextBuffer, PositionAffinity.Predecessor);
        }

        void QueryStatus(object sender, EventArgs e)
        {
            var tags = GetTags(GetCurrentTextViews());
            Enabled = tags.Count > 0;
        }

        protected interface ITagInfo
        {
            ITextView TextView { get; }
            ShowInlineCommentTag Tag { get; }
            SnapshotPoint Point { get; }
        }

        class TagInfo : ITagInfo
        {
            public ITextView TextView { get; set; }
            public ShowInlineCommentTag Tag { get; set; }
            public SnapshotPoint? Point { get; set; }

            SnapshotPoint ITagInfo.Point => Point.Value;
        }

        class TagInfoComparer : IComparer<ITagInfo>
        {
            public static readonly TagInfoComparer Instance = new TagInfoComparer();
            public int Compare(ITagInfo x, ITagInfo y) => x.Point.Position - y.Point.Position;
        }
    }
}
