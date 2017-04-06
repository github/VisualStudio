using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using GitHub.Services;
using GitHub.Models;

namespace GitHub.InlineReviews.Tags
{
    class ReviewTagger : ITagger<ReviewTag>, IDisposable
    {
        readonly ITextBuffer buffer;
        readonly IPullRequestReviewSessionManager sessionManager;
        readonly IDisposable subscription;
        List<InlineComment> comments;

        public ReviewTagger(ITextBuffer buffer, IPullRequestReviewSessionManager sessionManager)
        {
            this.buffer = buffer;
            this.sessionManager = sessionManager;
            this.buffer.Changed += Buffer_Changed;
            subscription = sessionManager.SessionChanged.Subscribe(SessionChanged);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public void Dispose()
        {
            subscription.Dispose();
        }

        public IEnumerable<ITagSpan<ReviewTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (comments != null)
            {
                foreach (var span in spans)
                {
                    var startLine = span.Start.GetContainingLine().LineNumber;
                    var endLine = span.End.GetContainingLine().LineNumber;

                    var spanComments = comments.Where(x =>
                        x.Position >= startLine &&
                        x.Position <= endLine);

                    foreach (var comment in spanComments)
                    {
                        var line = span.Snapshot.GetLineFromLineNumber(comment.Position);
                        yield return new TagSpan<ReviewTag>(
                            new SnapshotSpan(line.Start, line.End),
                            new ReviewTag(comment.Original));
                    }
                }
            }
        }

        void SessionChanged(IPullRequestReviewSession session)
        {
            if (session != null)
            {
                var document = buffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument));
                comments = CreateInlineComments(session.GetCommentsForFile(document.FilePath));

                var entireFile = new SnapshotSpan(buffer.CurrentSnapshot, 0, buffer.CurrentSnapshot.Length);
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(entireFile));
            }
            else
            {
                comments = null;
            }
        }

        List<InlineComment> CreateInlineComments(IEnumerable<IPullRequestReviewCommentModel> comments)
        {
            var result = new List<InlineComment>();

            foreach (var comment in comments)
            {
                if (comment.Position.HasValue)
                {
                    result.Add(new InlineComment(comment));
                }
            }

            return result;
        }

        void Buffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            if (comments != null)
            {
                foreach (var change in e.Changes)
                {
                    var line = buffer.CurrentSnapshot.GetLineFromPosition(change.OldPosition);

                    foreach (var comment in comments)
                    {
                        comment.UpdatePosition(line.LineNumber, change.LineCountDelta);
                    }
                }
            }
        }

        class InlineComment
        {
            public InlineComment(IPullRequestReviewCommentModel original)
            {
                Position = original.Position.Value - 1;
                Original = original;
            }

            public int Position { get; private set; }
            public IPullRequestReviewCommentModel Original { get; }

            public void UpdatePosition(int editLine, int editDelta)
            {
                if (Position >= editLine)
                {
                    Position += editDelta;
                }
            }
        }
    }
}
