using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.InlineReviews.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.Peek
{
    class ReviewPeekableResultSource : IPeekResultSource
    {
        readonly IPullRequestReviewSession session;
        readonly IList<InlineCommentModel> comments;

        public ReviewPeekableResultSource(
            IPullRequestReviewSession session,
            IList<InlineCommentModel> comments)
        {
            this.session = session;
            this.comments = comments;
        }

        public void FindResults(string relationshipName, IPeekResultCollection resultCollection, CancellationToken cancellationToken, IFindPeekResultsCallback callback)
        {
            resultCollection.Add(new ReviewPeekResult(session, comments));
        }
    }
}
