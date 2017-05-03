using System;
using System.Collections.Generic;
using System.Linq;
using GitHub.InlineReviews.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.Tags
{
    class ShowInlineCommentTag : InlineCommentTag
    {
        public ShowInlineCommentTag(IPullRequestReviewSession session, IEnumerable<InlineCommentModel> comments)
            : base(session)
        {
            Comments = new List<InlineCommentModel>(comments);
        }

        public IReadOnlyList<InlineCommentModel> Comments { get; }
        public bool IsAddNewCommentTag => Comments.Count == 0;
        public bool NeedsUpdate => Comments.Any(x => x.NeedsUpdate);
    }
}
