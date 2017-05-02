using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.InlineReviews.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekableResultSource : IPeekResultSource
    {
        readonly IPullRequestReviewSession session;
        readonly IList<InlineCommentModel> comments;

        public InlineCommentPeekableResultSource(
            IPullRequestReviewSession session,
            IList<InlineCommentModel> comments)
        {
            this.session = session;
            this.comments = comments;
        }

        public void FindResults(string relationshipName, IPeekResultCollection resultCollection, CancellationToken cancellationToken, IFindPeekResultsCallback callback)
        {
            resultCollection.Add(new InlineCommentPeekResult(session, comments));
        }
    }
}
