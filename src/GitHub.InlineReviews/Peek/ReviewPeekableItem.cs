using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.Models;

namespace GitHub.InlineReviews.Peek
{
    class ReviewPeekableItem : IPeekableItem
    {
        readonly IList<IPullRequestReviewCommentModel> comments;

        public ReviewPeekableItem(IList<IPullRequestReviewCommentModel> comments)
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
