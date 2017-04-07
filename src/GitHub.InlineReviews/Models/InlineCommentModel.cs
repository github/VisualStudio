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

        public void UpdatePosition(int editLine, int editDelta)
        {
            if (LineNumber >= editLine)
            {
                LineNumber += editDelta;
            }
        }
    }
}
