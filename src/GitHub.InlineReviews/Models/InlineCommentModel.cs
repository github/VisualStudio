using System;
using GitHub.Models;
using Microsoft.VisualStudio.Text;

namespace GitHub.InlineReviews.Models
{
    public class InlineCommentModel
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
        public bool NeedsUpdate { get; private set; }
        public ITrackingPoint TrackingPoint { get; }

        public bool Update(ITextSnapshot snapshot)
        {
            var position = TrackingPoint.GetPosition(snapshot);
            var lineNumber = snapshot.GetLineNumberFromPosition(position);

            if (lineNumber != LineNumber)
            {
                var result = !NeedsUpdate;
                LineNumber = lineNumber;
                NeedsUpdate = true;
                return result;
            }

            return false;
        }
    }
}
