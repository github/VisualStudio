using System;
using GitHub.Models;
using Microsoft.VisualStudio.Text;

namespace GitHub.InlineReviews.Models
{
    class InlineCommentModel : IInlineCommentModel
    {
        public InlineCommentModel(
            int lineNumber,
            IPullRequestReviewCommentModel original,
            ITrackingPoint trackingPoint)
        {
            LineNumber = lineNumber;
            Original = original;
            TrackingPoint = trackingPoint;
        }

        public int LineNumber { get; private set; }
        public IPullRequestReviewCommentModel Original { get; }
        public bool IsStale { get; private set; }
        public ITrackingPoint TrackingPoint { get; }

        public bool Update(ITextSnapshot snapshot)
        {
            if (Original == null)
            {
                IsStale = true;
                return true;
            }

            var position = TrackingPoint.GetPosition(snapshot);
            var lineNumber = snapshot.GetLineNumberFromPosition(position);

            if (lineNumber != LineNumber)
            {
                var result = !IsStale;
                LineNumber = lineNumber;
                IsStale = true;
                return result;
            }

            return false;
        }
    }
}
