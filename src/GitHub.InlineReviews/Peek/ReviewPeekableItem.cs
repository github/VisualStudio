using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.Models;
using GitHub.InlineReviews.Models;

namespace GitHub.InlineReviews.Peek
{
    class ReviewPeekableItem : IPeekableItem
    {
        readonly IList<InlineCommentModel> comments;

        public ReviewPeekableItem(IList<InlineCommentModel> comments)
        {
            this.comments = comments;
        }

        public string DisplayName => "GitHub Code Review";

        public IEnumerable<IPeekRelationship> Relationships => new[] { ReviewPeekRelationship.Instance };

        public IPeekResultSource GetOrCreateResultSource(string relationshipName)
        {
            return new ReviewPeekableResultSource(comments);
        }
    }
}
