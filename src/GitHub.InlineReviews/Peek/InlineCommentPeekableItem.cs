using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekableItem : IPeekableItem
    {
        public InlineCommentPeekableItem(InlineReviewPeekViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public string DisplayName => "GitHub Code Review";
        public InlineReviewPeekViewModel ViewModel { get; }

        public IEnumerable<IPeekRelationship> Relationships => new[] { InlineCommentPeekRelationship.Instance };

        public IPeekResultSource GetOrCreateResultSource(string relationshipName)
        {
            return new InlineCommentPeekableResultSource(ViewModel);
        }
    }
}
