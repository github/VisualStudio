using System;
using System.Collections.Generic;
using System.Linq;
using GitHub.Extensions;
using GitHub.InlineReviews.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.Tags
{
    class ShowInlineCommentTag : InlineCommentTag
    {
        public ShowInlineCommentTag(IPullRequestReviewSession session, IEnumerable<InlineCommentModel> comments)
            : base(session)
        {
            Guard.ArgumentNotNull(comments, nameof(comments));

            Comments = new List<InlineCommentModel>(comments);
        }

        public IReadOnlyList<InlineCommentModel> Comments { get; }
        public bool IsAddNewCommentTag => Comments.Count == 0;
        public bool NeedsUpdate => Comments.Any(x => x.IsStale);
    }
}
