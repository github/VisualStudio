using System;
using System.Collections.Generic;
using GitHub.InlineReviews.Models;

namespace GitHub.InlineReviews.Tags
{
    class InlineCommentBuilderResult
    {
        public InlineCommentBuilderResult(
            IReadOnlyList<InlineCommentModel> comments,
            IReadOnlyList<int> addCommentLines)
        {
            Comments = comments;
            AddCommentLines = addCommentLines;
        }

        public IReadOnlyList<InlineCommentModel> Comments { get; }
        public IReadOnlyList<int> AddCommentLines { get; }
    }
}
