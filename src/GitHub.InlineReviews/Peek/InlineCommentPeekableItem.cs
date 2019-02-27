using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.InlineReviews.ViewModels;
using GitHub.ViewModels;
using System.Reactive;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekableItem : IPeekableItem, IClosable
    {
        public InlineCommentPeekableItem(InlineCommentPeekViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public string DisplayName => "GitHub Code Review";
        public InlineCommentPeekViewModel ViewModel { get; }

        public IEnumerable<IPeekRelationship> Relationships => new[] { InlineCommentPeekRelationship.Instance };

        public IObservable<Unit> Closed => ViewModel.Close;

        public IPeekResultSource GetOrCreateResultSource(string relationshipName)
        {
            return new InlineCommentPeekableResultSource(ViewModel);
        }
    }
}
