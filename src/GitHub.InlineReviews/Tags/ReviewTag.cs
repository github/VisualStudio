using System;
using System.Collections.Generic;
using System.Linq;
using GitHub.InlineReviews.Models;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Tags
{
    class ReviewTag : IGlyphTag
    {
        public ReviewTag(IEnumerable<InlineCommentModel> comments)
        {
            Comments = new List<InlineCommentModel>(comments);
        }

        public IList<InlineCommentModel> Comments { get; }
        public bool NeedsUpdate => Comments.Any(x => x.NeedsUpdate);
    }
}
