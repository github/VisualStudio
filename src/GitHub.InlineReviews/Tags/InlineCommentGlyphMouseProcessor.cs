using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
        readonly ITagAggregator<ShowInlineCommentTag> tagAggregator;

        public InlineCommentGlyphMouseProcessor(
            IPeekBroker peekBroker,
            ITextView textView,
            IWpfTextViewMargin margin,
            ITagAggregator<ShowInlineCommentTag> aggregator)
        {
            this.peekBroker = peekBroker;
            this.textView = textView;
            this.margin = margin;
            this.tagAggregator = aggregator;
        }

        public override void PreprocessMouseMove(MouseEventArgs e)
        {
            var p = e.GetPosition(margin.VisualElement);
            var hit = VisualTreeHelper.HitTest(margin.VisualElement, p);
            
            if (hit != null)
            {
                p = e.GetPosition((IInputElement)hit.VisualHit);
                hit = VisualTreeHelper.HitTest((Visual)hit.VisualHit, p);
                System.Diagnostics.Debug.WriteLine(hit?.VisualHit?.GetType().Name);
            }
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
