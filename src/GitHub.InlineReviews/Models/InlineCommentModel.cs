using System;
using GitHub.Models;

namespace GitHub.InlineReviews.Models
{
    public class InlineCommentModel
    {
        public InlineCommentModel(int lineNumber, IPullRequestReviewCommentModel original)
        {
            LineNumber = lineNumber;
            Original = original;
        }

        public int LineNumber { get; private set; }
        public IPullRequestReviewCommentModel Original { get; }
        public bool NeedsUpdate { get; private set; }

        public bool UpdatePosition(int editLine, int editDelta)
        {
            if (LineNumber >= editLine)
            {
                var result = !NeedsUpdate;
                LineNumber += editDelta;
                NeedsUpdate = true;
                return result;
            }

            return false;
        }
    }
}
