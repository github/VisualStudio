using System;
using System.Collections.Generic;
using GitHub.Models;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Tags
{
    class ReviewTag : IGlyphTag
    {
        public ReviewTag(IEnumerable<IPullRequestReviewCommentModel> comments)
        {
            Comments = new List<IPullRequestReviewCommentModel>(comments);
        }

        public IList<IPullRequestReviewCommentModel> Comments { get; }
    }
}
