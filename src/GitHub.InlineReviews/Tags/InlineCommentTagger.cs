using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.InlineReviews.Services;
using GitHub.Models;
using GitHub.Services;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
namespace GitHub.InlineReviews.Tags
{
    /// <summary>
    /// Creates tags in an <see cref="ITextBuffer"/> for inline comment threads.
    /// </summary>
    sealed class InlineCommentTagger : ITagger<InlineCommentTag>, IEditorContentSource, IDisposable
    {
        static readonly IReadOnlyList<ITagSpan<InlineCommentTag>> EmptyTags = new ITagSpan<InlineCommentTag>[0];
        readonly IGitService gitService;
        readonly IGitClient gitClient;
        readonly IDiffService diffService;
        readonly ITextBuffer buffer;
        readonly ITextView view;
        readonly IPullRequestSessionManager sessionManager;
        readonly IInlineCommentPeekService peekService;
        bool needsInitialize = true;
        string relativePath;
        bool leftHandSide;
        IPullRequestSession session;
        IPullRequestSessionFile file;
        IDisposable fileSubscription;

        public InlineCommentTagger(
            IGitService gitService,
            IGitClient gitClient,
            IDiffService diffService,
            ITextView view,
            ITextBuffer buffer,
            IPullRequestSessionManager sessionManager,
            IInlineCommentPeekService peekService)
        {
            Guard.ArgumentNotNull(gitService, nameof(gitService));
            Guard.ArgumentNotNull(gitClient, nameof(gitClient));
            Guard.ArgumentNotNull(diffService, nameof(diffService));
            Guard.ArgumentNotNull(buffer, nameof(buffer));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));
            Guard.ArgumentNotNull(peekService, nameof(peekService));

            this.gitService = gitService;
            this.gitClient = gitClient;
            this.diffService = diffService;
            this.buffer = buffer;
            this.view = view;
            this.sessionManager = sessionManager;
            this.peekService = peekService;
        }

        public bool ShowMargin => file != null;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public void Dispose()
        {
            fileSubscription?.Dispose();
            fileSubscription = null;
        }

        public IEnumerable<ITagSpan<InlineCommentTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (needsInitialize)
            {
                // Sucessful initialization will call NotifyTagsChanged, causing this method to be re-called.
                ForgetWithLogging(Initialize());
                return EmptyTags;
            }
            else if (file != null)
            {
                var result = new List<ITagSpan<InlineCommentTag>>();
                var currentSession = session ?? sessionManager.CurrentSession;

                if (currentSession == null)
                    return EmptyTags;

                foreach (var span in spans)
                {
                    var startLine = span.Start.GetContainingLine().LineNumber;
                    var endLine = span.End.GetContainingLine().LineNumber;
                    var linesWithComments = new BitArray((endLine - startLine) + 1);
                    var spanThreads = file.InlineCommentThreads.Where(x =>
                        x.LineNumber >= startLine &&
                        x.LineNumber <= endLine);

                    foreach (var thread in spanThreads)
                    {
                        var snapshot = span.Snapshot;
                        var line = snapshot.GetLineFromLineNumber(thread.LineNumber);

                        if ((leftHandSide && thread.DiffLineType == DiffChangeType.Delete) ||
                            (!leftHandSide && thread.DiffLineType != DiffChangeType.Delete))
                        {
                            linesWithComments[thread.LineNumber - startLine] = true;

                            result.Add(new TagSpan<ShowInlineCommentTag>(
                                new SnapshotSpan(line.Start, line.End),
                                new ShowInlineCommentTag(currentSession, thread)));
                        }
                    }

                    foreach (var chunk in file.Diff)
                    {
                        foreach (var line in chunk.Lines)
                        {
                            var lineNumber = (leftHandSide ? line.OldLineNumber : line.NewLineNumber) - 1;

                            if (lineNumber >= startLine &&
                                lineNumber <= endLine &&
                                !linesWithComments[lineNumber - startLine]
                                && (!leftHandSide || line.Type == DiffChangeType.Delete))
                            {
                                var snapshotLine = span.Snapshot.GetLineFromLineNumber(lineNumber);
                                result.Add(new TagSpan<InlineCommentTag>(
                                    new SnapshotSpan(snapshotLine.Start, snapshotLine.End),
                                    new AddInlineCommentTag(currentSession, file.CommitSha, relativePath, line.DiffLineNumber, lineNumber, line.Type)));
                            }
                        }
                    }
                }

                return result;
            }
            else
            {
                return EmptyTags;
            }
        }

        Task<byte[]> IEditorContentSource.GetContent()
        {
            throw new NotImplementedException();
        }

        async Task Initialize()
        {
            needsInitialize = false;

            var bufferInfo = sessionManager.GetTextBufferInfo(buffer);

            if (bufferInfo != null)
            {
                session = bufferInfo.Session;
                relativePath = bufferInfo.RelativePath;
                file = await session.GetFile(relativePath);
                leftHandSide = bufferInfo.IsLeftComparisonBuffer;
            }
            else
            {
                relativePath = sessionManager.GetRelativePath(buffer);

                if (relativePath != null)
                {
                    var liveFile = await sessionManager.GetLiveFile(relativePath, view);
                    liveFile.LinesChanged.Subscribe(NotifyTagsChanged);
                    file = liveFile;
                }
                else
                {
                    file = null;
                }
            }

            NotifyTagsChanged();
        }

        static void ForgetWithLogging(Task task)
        {
            task.Catch(e => VsOutputLogger.WriteLine("Exception caught while executing background task: {0}", e)).Forget();
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

        void NotifyTagsChanged(IEnumerable<int> lineNumbers)
        {
            foreach (var lineNumber in lineNumbers)
            {
                NotifyTagsChanged(lineNumber);
            }
        }
    }
}
