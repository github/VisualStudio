using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using GitHub.InlineReviews.Tags;

namespace GitHub.InlineReviews.Peek
{
    [Export(typeof(IPeekableItemSourceProvider))]
    [ContentType("text")]
    [Name("GitHub Peekable Review Provider")]
    class ReviewPeekableItemSourceProvider : IPeekableItemSourceProvider
    {
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;

        [ImportingConstructor]
        public ReviewPeekableItemSourceProvider(IViewTagAggregatorFactoryService tagAggregatorFactory)
        {
            this.tagAggregatorFactory = tagAggregatorFactory;
        }

        public IPeekableItemSource TryCreatePeekableItemSource(ITextBuffer textBuffer)
        {
            return new ReviewPeekableItemSource(textBuffer, tagAggregatorFactory);
        }
    }
}
