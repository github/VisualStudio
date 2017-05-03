using System;
using System.Collections.Generic;
using GitHub.InlineReviews.Models;

namespace GitHub.InlineReviews.Tags
{
    class InlineCommentBuilderResult
    {
        public InlineCommentBuilderResult(
            IReadOnlyList<InlineCommentModel> comments,
            IReadOnlyList<AddCommentModel> addComments)
        {
            Comments = comments;
            AddComments = addComments;
        }

        public IReadOnlyList<InlineCommentModel> Comments { get; }
        public IReadOnlyList<AddCommentModel> AddComments { get; }
    }
}
