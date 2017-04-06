using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using GitHub.InlineReviews.Tags;

namespace GitHub.InlineReviews.Peek
{
    class ReviewPeekableItemSource : IPeekableItemSource
    {
        readonly ITextBuffer textBuffer;
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;

        public ReviewPeekableItemSource(
            ITextBuffer textBuffer,
            IViewTagAggregatorFactoryService tagAggregatorFactory)
        {
            this.textBuffer = textBuffer;
            this.tagAggregatorFactory = tagAggregatorFactory;
        }

        public void AugmentPeekSession(IPeekSession session, IList<IPeekableItem> peekableItems)
        {
            if (session.RelationshipName == ReviewPeekRelationship.Instance.Name)
            {
                var triggerPoint = session.GetTriggerPoint(textBuffer.CurrentSnapshot);

                if (triggerPoint.HasValue)
                {
                    var line = session.TextView.GetTextViewLineContainingBufferPosition(triggerPoint.Value);

                    if (line != null)
                    {
                        var aggregator = tagAggregatorFactory.CreateTagAggregator<ReviewTag>(session.TextView);
                        var comments = aggregator.GetTags(line.ExtentAsMappingSpan).Select(x => x.Tag.Comment);
                        peekableItems.Add(new ReviewPeekableItem(comments.ToList()));
                    }
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
