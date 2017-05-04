using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekableItem : IPeekableItem
    {
        readonly InlineCommentThreadViewModel viewModel;

        public InlineCommentPeekableItem(InlineCommentThreadViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public string DisplayName => "GitHub Code Review";

        public IEnumerable<IPeekRelationship> Relationships => new[] { InlineCommentPeekRelationship.Instance };

        public IPeekResultSource GetOrCreateResultSource(string relationshipName)
        {
            return new InlineCommentPeekableResultSource(viewModel);
        }
    }
}
