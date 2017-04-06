using System;
using GitHub.Models;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Tags
{
    class ReviewTag : IGlyphTag
    {
        public ReviewTag(IPullRequestReviewCommentModel comment)
        {
            Comment = comment;
        }

        public IPullRequestReviewCommentModel Comment { get; }
    }
}
