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
    abstract class InlineCommentNavigationCommand : Command
    {
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;
        readonly IInlineCommentPeekService peekService;

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

        protected IReadOnlyList<ITagInfo> GetTags(ITextView textView)
        {
            var tagAggregator = tagAggregatorFactory.CreateTagAggregator<InlineCommentTag>(textView);
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

        protected void ShowPeekComments(ShowInlineCommentTag tag)
        {
            peekService.Show(tag, true);
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
