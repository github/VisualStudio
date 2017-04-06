using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using GitHub.Services;

namespace GitHub.InlineReviews.Tags
{
    class ReviewTagger : ITagger<ReviewTag>, IDisposable
    {
        readonly ITextBuffer buffer;
        readonly IPullRequestReviewSessionManager sessionManager;
        readonly IDisposable subscription;
        IPullRequestReviewSession session;

        public ReviewTagger(ITextBuffer buffer, IPullRequestReviewSessionManager sessionManager)
        {
            this.buffer = buffer;
            this.sessionManager = sessionManager;
            subscription = sessionManager.SessionChanged.Subscribe(SessionChanged);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public void Dispose()
        {
            subscription.Dispose();
        }

        public IEnumerable<ITagSpan<ReviewTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (session != null)
            {
                var document = buffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument));
                var comments = session.GetCommentsForFile(document.FilePath);

                foreach (var span in spans)
                {
                    // Line numbers here are 0-based but PullRequestReviewComment.Position is 1-based.
                    var startLine = span.Start.GetContainingLine().LineNumber + 1;
                    var endLine = span.End.GetContainingLine().LineNumber + 1;

                    var spanComments = comments.Where(x =>
                        x.Position.HasValue &&
                        x.Position >= startLine &&
                        x.Position <= endLine);

                    foreach (var comment in spanComments)
                    {
                        var line = span.Snapshot.GetLineFromLineNumber(comment.Position.Value - 1);
                        yield return new TagSpan<ReviewTag>(
                            new SnapshotSpan(line.Start, line.End),
                            new ReviewTag(comment));
                    }
                }
            }
        }

        void SessionChanged(IPullRequestReviewSession session)
        {
            this.session = session;
            var entireFile = new SnapshotSpan(buffer.CurrentSnapshot, 0, buffer.CurrentSnapshot.Length);
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(entireFile));
        }
    }
}
