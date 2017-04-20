using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using GitHub.Services;
using GitHub.Models;
using System.IO;
using LibGit2Sharp;
using GitHub.InlineReviews.Models;
using GitHub.InlineReviews.Services;
using System.Threading.Tasks;

namespace GitHub.InlineReviews.Tags
{
    class ReviewTagger : ITagger<ReviewTag>, IDisposable
    {
        readonly ITextBuffer buffer;
        readonly IPullRequestReviewSessionManager sessionManager;
        readonly IInlineCommentBuilder builder;
        readonly IDisposable subscription;
        string path;
        IList<InlineCommentModel> comments;

        public ReviewTagger(
            ITextBuffer buffer,
            IPullRequestReviewSessionManager sessionManager,
            IInlineCommentBuilder builder)
        {
            this.buffer = buffer;
            this.sessionManager = sessionManager;
            this.builder = builder;
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
                        x.LineNumber >= startLine &&
                        x.LineNumber <= endLine)
                        .GroupBy(x => x.LineNumber);

                    foreach (var entry in spanComments)
                    {
                        var line = span.Snapshot.GetLineFromLineNumber(entry.Key);
                        yield return new TagSpan<ReviewTag>(
                            new SnapshotSpan(line.Start, line.End),
                            new ReviewTag(entry.Select(x => x.Original)));
                    }
                }
            }
        }

        static string RootedPathToRelativePath(string path, string basePath)
        {
            if (Path.IsPathRooted(path))
            {
                if (path.StartsWith(basePath) && path.Length > basePath.Length + 1)
                {
                    return path.Substring(basePath.Length + 1);
                }
            }

            return null;
        }

        void NotifyTagsChanged()
        {
            var entireFile = new SnapshotSpan(buffer.CurrentSnapshot, 0, buffer.CurrentSnapshot.Length);
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(entireFile));
        }

        async void SessionChanged(IPullRequestReviewSession session)
        {
            comments = null;
            NotifyTagsChanged();

            if (session != null)
            {
                var document = buffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument));
                path = RootedPathToRelativePath(document.FilePath, session.Repository.LocalPath);
                if(path == null)
                {
                    // Ignore files outside of repo.
                    return;
                }

                comments = await builder.Build(path, buffer.CurrentSnapshot, session);
                NotifyTagsChanged();
            }
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
    }
}
