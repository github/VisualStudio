using System;
using GitHub.Services;

namespace GitHub.InlineReviews.Tags
{
    class AddInlineCommentTag : InlineCommentTag
    {
        public AddInlineCommentTag(IPullRequestReviewSession session)
            : base(session)
        {
        }
    }
}
