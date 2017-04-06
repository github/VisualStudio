using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.Models;

namespace GitHub.InlineReviews.Peek
{
    class ReviewPeekableResultSource : IPeekResultSource
    {
        readonly IList<IPullRequestReviewCommentModel> comments;

        public ReviewPeekableResultSource(IList<IPullRequestReviewCommentModel> comments)
        {
            this.comments = comments;
        }

        public void FindResults(string relationshipName, IPeekResultCollection resultCollection, CancellationToken cancellationToken, IFindPeekResultsCallback callback)
        {
            resultCollection.Add(new ReviewPeekResult(comments));
        }
    }
}
