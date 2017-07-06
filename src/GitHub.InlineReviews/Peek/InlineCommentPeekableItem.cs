using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekableItem : IPeekableItem
    {
        public InlineCommentPeekableItem(InlineCommentPeekViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public string DisplayName => "GitHub Code Review";
        public InlineCommentPeekViewModel ViewModel { get; }

        public IEnumerable<IPeekRelationship> Relationships => new[] { InlineCommentPeekRelationship.Instance };

        public IPeekResultSource GetOrCreateResultSource(string relationshipName)
        {
            return new InlineCommentPeekableResultSource(ViewModel);
        }
    }
}
