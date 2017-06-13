using System;
using System.Collections.Generic;
using System.Linq;
using GitHub.Extensions;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.Tags;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.TextManager.Interop;

namespace GitHub.InlineReviews.Commands
{
    /// <summary>
    /// Base class for commands that navigate between inline comments.
    /// </summary>
    abstract class InlineCommentNavigationCommand : VsCommand<InlineCommentNavigationParams>
    {
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;
        readonly IInlineCommentPeekService peekService;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCommentNavigationCommand"/> class.
        /// </summary>
        /// <param name="tagAggregatorFactory">The tag aggregator factory.</param>
        /// <param name="peekService">The peek service.</param>
        /// <param name="commandSet">The GUID of the group the command belongs to.</param>
        /// <param name="commandId">The numeric identifier of the command.</param>
        protected InlineCommentNavigationCommand(
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            IInlineCommentPeekService peekService,
            Guid commandSet,
            int commandId)
            : base(commandSet, commandId)
        {
            this.tagAggregatorFactory = tagAggregatorFactory;
            this.peekService = peekService;
        }

        /// <inheritdoc/>
        public override bool IsEnabled
        {
            get
            {
                var textView = GetCurrentTextView();
                if (textView == null) return false;

                var tagAggregator = tagAggregatorFactory.CreateTagAggregator<InlineCommentTag>(textView);
                var span = new SnapshotSpan(textView.TextSnapshot, 0, textView.TextSnapshot.Length);
                var mappingSpan = textView.BufferGraph.CreateMappingSpan(span, SpanTrackingMode.EdgeExclusive);
                return tagAggregator.GetTags(mappingSpan).Any();
            }
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
        /// <param name="parameter">The parameters.</param>
        /// <param name="lineNumber">The 0-based line number.</param>
        /// <returns></returns>
        protected int GetCursorPoint(ITextView textView, int lineNumber)
        {
            lineNumber = Math.Max(0, Math.Min(lineNumber, textView.TextSnapshot.LineCount - 1));
            return textView.TextSnapshot.GetLineFromLineNumber(lineNumber).Start.Position;
        }

        /// <summary>
        /// Gets the currently active text view from Visual Studio.
        /// </summary>
        /// <returns>An <see cref="ITextView"/> or null if no active text view.</returns>
        protected ITextView GetCurrentTextView()
        {
            IVsTextView textView = null;
            var textManager = (IVsTextManager)Package.GetServiceSafe(typeof(VsTextManagerClass));
            textManager.GetActiveView(0, null, out textView);
            var userData = textView as IVsUserData;
            if (userData == null) return null;
            IWpfTextViewHost viewHost;
            object holder;
            Guid guidViewHost = DefGuidList.guidIWpfTextViewHost;
            userData.GetData(ref guidViewHost, out holder);
            viewHost = (IWpfTextViewHost)holder;
            return viewHost?.TextView;
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
        /// <param name="textView">The text view.</param>
        /// <returns>A collection of <see cref="ITagInfo"/> objects, ordered by line.</returns>
        protected IReadOnlyList<ITagInfo> GetTags(ITextView textView)
        {
            var tagAggregator = CreateTagAggregator(textView);
            var span = new SnapshotSpan(textView.TextSnapshot, 0, textView.TextSnapshot.Length);
            var mappingSpan = textView.BufferGraph.CreateMappingSpan(span, SpanTrackingMode.EdgeExclusive);

            return tagAggregator.GetTags(mappingSpan)
                .Select(x => new TagInfo
                {
                    Point = Map(x.Span.Start, textView.TextSnapshot),
                    Tag = x.Tag as ShowInlineCommentTag,
                })
                .Where(x => x.Tag != null && x.Point.HasValue)
                .OrderBy(x => x.Point)
                .ToList();
        }

        /// <summary>
        /// Shows the inline comments for the specified tag in a peek view.
        /// </summary>
        /// <param name="tag"></param>
        protected void ShowPeekComments(
            InlineCommentNavigationParams parameter,
            ITextView textView,
            ShowInlineCommentTag tag)
        {
            peekService.Show(textView, tag);

            if (parameter?.MoveCursor != false)
            {
                var point = new SnapshotPoint(textView.TextSnapshot, GetCursorPoint(textView, tag.LineNumber));
                textView.Caret.MoveTo(point);
            }
        }

        SnapshotPoint? Map(IMappingPoint p, ITextSnapshot textSnapshot)
        {
            return p.GetPoint(textSnapshot.TextBuffer, PositionAffinity.Predecessor);
        }

        protected interface ITagInfo
        {
            ShowInlineCommentTag Tag { get; }
            SnapshotPoint Point { get; }
        }

        private class TagInfo : ITagInfo
        {
            public ShowInlineCommentTag Tag { get; set; }
            public SnapshotPoint? Point { get; set; }

            SnapshotPoint ITagInfo.Point => Point.Value;
        }
    }
}
