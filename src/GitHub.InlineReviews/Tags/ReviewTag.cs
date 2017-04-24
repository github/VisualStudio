using System;
using System.Collections.Generic;
using System.Linq;
using GitHub.InlineReviews.Models;
using GitHub.Services;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Tags
{
    class ReviewTag : IGlyphTag
    {
        public ReviewTag(IPullRequestReviewSession session, IEnumerable<InlineCommentModel> comments)
        {
            Comments = new List<InlineCommentModel>(comments);
            Session = session;
        }

        public IList<InlineCommentModel> Comments { get; }
        public bool NeedsUpdate => Comments.Any(x => x.NeedsUpdate);
        public IPullRequestReviewSession Session { get; }
    }
}
