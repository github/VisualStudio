using System;
using System.Linq;
using System.Windows.Input;
using GitHub.InlineReviews.Peek;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace GitHub.InlineReviews.Tags
{
    class InlineCommentGlyphMouseProcessor : MouseProcessorBase
    {
        readonly IPeekBroker peekBroker;
        readonly ITextView textView;
        readonly IWpfTextViewMargin margin;
        readonly ITagAggregator<InlineCommentTag> tagAggregator;

        public InlineCommentGlyphMouseProcessor(
            IPeekBroker peekBroker,
            ITextView textView,
            IWpfTextViewMargin margin,
            ITagAggregator<InlineCommentTag> aggregator)
        {
            this.peekBroker = peekBroker;
            this.textView = textView;
            this.margin = margin;
            this.tagAggregator = aggregator;
        }

        public override void PreprocessMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            var y = e.GetPosition(margin.VisualElement).Y + textView.ViewportTop;
            var line = textView.TextViewLines.GetTextViewLineContainingYCoordinate(y);

            if (line != null)
            {
                var tag = tagAggregator.GetTags(line.ExtentAsMappingSpan).FirstOrDefault();

                if (tag != null)
                {
                    var trackingPoint = textView.TextSnapshot.CreateTrackingPoint(line.Start.Position, PointTrackingMode.Positive);
                    var options = new InlineCommentPeekSessionCreationOptions(textView, trackingPoint, tag.Tag.Session, tag.Tag.Comments);
                    var session = peekBroker.TriggerPeekSession(options);
                    e.Handled = true;
                }
            }
        }
    }
}
