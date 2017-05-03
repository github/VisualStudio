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
using GitHub.Models;
using GitHub.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Text.Tagging;
using ReactiveUI;

namespace GitHub.InlineReviews.Tags
{
    sealed class InlineCommentTagger : ITagger<InlineCommentTag>, IDisposable
    {
        readonly IGitService gitService;
        readonly IGitClient gitClient;
        readonly IDiffService diffService;
        readonly ITextBuffer buffer;
        readonly ITextView view;
        readonly IPullRequestReviewSessionManager sessionManager;
        readonly Subject<ITextSnapshot> signalRebuild;
        readonly int? tabsToSpaces;
        bool initialized;
        string fullPath;
        bool leftHandSide;
        IDisposable subscription;
        IPullRequestReviewSession session;
        InlineCommentBuilder commentBuilder;
        IList<InlineCommentModel> comments;
        IList<int> addCommentLines;

        public InlineCommentTagger(
            IGitService gitService,
            IGitClient gitClient,
            IDiffService diffService,
            ITextView view,
            ITextBuffer buffer,
            IPullRequestReviewSessionManager sessionManager)
        {
            Guard.ArgumentNotNull(gitService, nameof(gitService));
            Guard.ArgumentNotNull(gitClient, nameof(gitClient));
            Guard.ArgumentNotNull(diffService, nameof(diffService));
            Guard.ArgumentNotNull(buffer, nameof(buffer));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));

            this.gitService = gitService;
            this.gitClient = gitClient;
            this.diffService = diffService;
            this.buffer = buffer;
            this.view = view;
            this.sessionManager = sessionManager;

            if (view.Options.GetOptionValue("Tabs/ConvertTabsToSpaces", false))
            {
                tabsToSpaces = view.Options.GetOptionValue<int?>("Tabs/TabSize", null);
            }

            signalRebuild = new Subject<ITextSnapshot>();
            signalRebuild.Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => Rebuild(x).Forget());

            this.buffer.Changed += Buffer_Changed;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public void Dispose()
        {
            subscription?.Dispose();
        }

        public IEnumerable<ITagSpan<InlineCommentTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (!initialized)
            {
                // Sucessful initialization will call NotifyTagsChanged, causing this method to be re-called.
                Initialize();
            }
            else if (comments != null)
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
                        yield return new TagSpan<InlineCommentTag>(
                            new SnapshotSpan(line.Start, line.End),
                            new InlineCommentTag(session, entry));
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

        void Initialize()
        {
            var bufferTag = buffer.Properties.GetProperty<CompareBufferTag>(typeof(CompareBufferTag), null);

            if (bufferTag != null)
            {
                fullPath = bufferTag.Path;
                leftHandSide = bufferTag.IsLeftBuffer;
            }
            else
            {
                var document = buffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument));
                fullPath = document.FilePath;
            }

            subscription = sessionManager.SessionChanged
                .SelectMany(x => Observable.Return(x).Concat(x.Changed.Select(_ => x)))
                .Subscribe(SessionChanged);

            initialized = true;
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

            if (comments != null)
            {
                comments = null;
                NotifyTagsChanged();
            }

            if (session == null) return;

            string path = RootedPathToRelativePath(fullPath, session.Repository.LocalPath);

            if (path == null) return;

            var snapshot = buffer.CurrentSnapshot;

            if (leftHandSide)
            {
                // If we're tagging the LHS of a diff, then the snapshot will be the base commit
                // (as you'd expect) but that means that the diff will be empty, so get the RHS
                // snapshot from the view for the comparison.
                var projection = view.TextSnapshot as IProjectionSnapshot;
                snapshot = projection?.SourceSnapshots.Count == 2 ? projection.SourceSnapshots[1] : null;
            }

            if (snapshot == null) return;

            var repository = gitService.GetRepository(session.Repository.LocalPath);
            commentBuilder = new InlineCommentBuilder(
                gitClient,
                diffService,
                session,
                repository,
                path,
                leftHandSide,
                tabsToSpaces);

            comments = await commentBuilder.BuildComments(snapshot);
            addCommentLines = await commentBuilder.GetAddCommentLines(snapshot);

            NotifyTagsChanged();
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
                var newComments = await commentBuilder.BuildComments(snapshot);
                var newCommentLines = await commentBuilder.GetAddCommentLines(snapshot);

                if (buffer.CurrentSnapshot == snapshot)
                {
                    comments = newComments;
                    addCommentLines = newCommentLines;
                    NotifyTagsChanged();
                }
            }
        }
    }
}
