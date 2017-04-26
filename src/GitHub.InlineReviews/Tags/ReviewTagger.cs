using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.InlineReviews.Models;
using GitHub.InlineReviews.Services;
using GitHub.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using ReactiveUI;

namespace GitHub.InlineReviews.Tags
{
    sealed class ReviewTagger : ITagger<ReviewTag>, IDisposable
    {
        readonly IGitService gitService;
        readonly IGitClient gitClient;
        readonly ITextBuffer buffer;
        readonly IPullRequestReviewSessionManager sessionManager;
        readonly IDisposable subscription;
        readonly Subject<ITextSnapshot> signalRebuild;
        readonly int? tabsToSpaces;
        IPullRequestReviewSession session;
        InlineCommentBuilder commentBuilder;
        IList<InlineCommentModel> comments;

        public ReviewTagger(
            IGitService gitService,
            IGitClient gitClient,
            ITextView view,
            ITextBuffer buffer,
            IPullRequestReviewSessionManager sessionManager)
        {
            Guard.ArgumentNotNull(gitService, nameof(gitService));
            Guard.ArgumentNotNull(gitClient, nameof(gitClient));
            Guard.ArgumentNotNull(buffer, nameof(buffer));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));

            this.gitService = gitService;
            this.gitClient = gitClient;
            this.buffer = buffer;
            this.sessionManager = sessionManager;

            if (view.Options.GetOptionValue<bool>("Tabs/ConvertTabsToSpaces"))
            {
                tabsToSpaces = view.Options.GetOptionValue<int>("Tabs/TabSize");
            }

            signalRebuild = new Subject<ITextSnapshot>();
            signalRebuild.Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => Rebuild(x).Forget());

            this.buffer.Changed += Buffer_Changed;
            subscription = sessionManager.SessionChanged
                .SelectMany(x => Observable.Return(x).Concat(x.Changed.Select(_ => x)))
                .Subscribe(SessionChanged);
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
                            new ReviewTag(session, entry));
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

        void NotifyTagsChanged(int lineNumber)
        {
            var line = buffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber);
            var span = new SnapshotSpan(buffer.CurrentSnapshot, line.Start, line.Length);
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
        }

        async void SessionChanged(IPullRequestReviewSession session)
        {
            this.session = session;
            comments = null;
            NotifyTagsChanged();

            if (session != null)
            {
                string path;

                if (session.CompareViewHackPath == null)
                {
                    var document = buffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument));
                    path = RootedPathToRelativePath(document.FilePath, session.Repository.LocalPath);
                }
                else
                {
                    path = session.CompareViewHackPath;
                }

                if (path != null)
                {
                    var repository = gitService.GetRepository(session.Repository.LocalPath);
                    commentBuilder = new InlineCommentBuilder(gitClient, session, repository, path, tabsToSpaces);
                    comments = await commentBuilder.Update(buffer.CurrentSnapshot);
                    NotifyTagsChanged();
                }
            }
        }

        void Buffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            if (comments != null)
            {
                foreach (var comment in comments)
                {
                    if (comment.Update(buffer.CurrentSnapshot))
                    {
                        NotifyTagsChanged(comment.LineNumber);
                    }
                }
            }

            signalRebuild.OnNext(buffer.CurrentSnapshot);
        }

        async Task Rebuild(ITextSnapshot snapshot)
        {
            if (buffer.CurrentSnapshot == snapshot)
            {
                var updated = await commentBuilder.Update(snapshot);

                if (buffer.CurrentSnapshot == snapshot)
                {
                    comments = updated;
                    NotifyTagsChanged();
                }
            }
        }
    }
}
