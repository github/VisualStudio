using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using GitHub.InlineReviews.Tags;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekableItemSource : IPeekableItemSource
    {
        readonly ITextBuffer textBuffer;
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;

        public InlineCommentPeekableItemSource(
            ITextBuffer textBuffer,
            IViewTagAggregatorFactoryService tagAggregatorFactory)
        {
            this.textBuffer = textBuffer;
            this.tagAggregatorFactory = tagAggregatorFactory;
        }

        public void AugmentPeekSession(IPeekSession session, IList<IPeekableItem> peekableItems)
        {
            var options = session.CreationOptions as InlineCommentPeekSessionCreationOptions;

            if (session.RelationshipName == InlineCommentPeekRelationship.Instance.Name && options != null)
            {
                peekableItems.Add(new InlineCommentPeekableItem(options.Session, options.Comments));
            }
        }

        public void Dispose()
        {
        }
    }
}
