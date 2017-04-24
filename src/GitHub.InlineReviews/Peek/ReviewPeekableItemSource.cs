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
            var options = session.CreationOptions as ReviewPeekSessionCreationOptions;

            if (session.RelationshipName == ReviewPeekRelationship.Instance.Name && options != null)
            {
                peekableItems.Add(new ReviewPeekableItem(options.Session, options.Comments));
            }
        }

        public void Dispose()
        {
        }
    }
}
