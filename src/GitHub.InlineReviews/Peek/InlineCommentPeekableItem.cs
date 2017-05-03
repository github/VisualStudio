using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.InlineReviews.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekableItem : IPeekableItem
    {
        readonly IPullRequestReviewSession session;
        readonly IReadOnlyList<InlineCommentModel> comments;

        public InlineCommentPeekableItem(
            IPullRequestReviewSession session,
            IReadOnlyList<InlineCommentModel> comments)
        {
            this.session = session;
            this.comments = comments;
        }

        public string DisplayName => "GitHub Code Review";

        public IEnumerable<IPeekRelationship> Relationships => new[] { InlineCommentPeekRelationship.Instance };

        public IPeekResultSource GetOrCreateResultSource(string relationshipName)
        {
            return new InlineCommentPeekableResultSource(session, comments);
        }
    }
}
