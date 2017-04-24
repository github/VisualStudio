using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.InlineReviews.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.Peek
{
    class ReviewPeekableItem : IPeekableItem
    {
        readonly IPullRequestReviewSession session;
        readonly IList<InlineCommentModel> comments;

        public ReviewPeekableItem(
            IPullRequestReviewSession session,
            IList<InlineCommentModel> comments)
        {
            this.session = session;
            this.comments = comments;
        }

        public string DisplayName => "GitHub Code Review";

        public IEnumerable<IPeekRelationship> Relationships => new[] { ReviewPeekRelationship.Instance };

        public IPeekResultSource GetOrCreateResultSource(string relationshipName)
        {
            return new ReviewPeekableResultSource(session, comments);
        }
    }
}
