using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using GitHub.InlineReviews.Margins;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using ReactiveUI;
using Serilog;

namespace GitHub.InlineReviews.Tags
{
    /// <summary>
    /// Creates tags in an <see cref="ITextBuffer"/> for inline comment threads.
    /// </summary>
    public sealed class InlineCommentTagger : ITagger<InlineTag>, IDisposable
    {
        static readonly ILogger log = LogManager.ForContext<InlineCommentTagger>();
        static readonly IReadOnlyList<ITagSpan<InlineTag>> EmptyTags = new ITagSpan<InlineTag>[0];
        readonly ITextBuffer buffer;
        readonly ITextView view;
        readonly IPullRequestSessionManager sessionManager;
        bool needsInitialize = true;
        string relativePath;
        DiffSide side;
        IPullRequestSession session;
        IPullRequestSessionFile file;
        IDisposable fileSubscription;
        IDisposable sessionManagerSubscription;
        IDisposable visibleSubscription;

        public InlineCommentTagger(
            ITextView view,
            ITextBuffer buffer,
            IPullRequestSessionManager sessionManager)
        {
            Guard.ArgumentNotNull(buffer, nameof(buffer));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));

            this.buffer = buffer;
            this.view = view;
            this.sessionManager = sessionManager;
        }

        public bool ShowMargin => file?.Diff?.Count > 0;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public void Dispose()
        {
            sessionManagerSubscription?.Dispose();
            sessionManagerSubscription = null;
            fileSubscription?.Dispose();
            fileSubscription = null;
            visibleSubscription?.Dispose();
            visibleSubscription = null;
        }

        public IEnumerable<ITagSpan<InlineTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (needsInitialize)
            {
                // Sucessful initialization will call NotifyTagsChanged, causing this method to be re-called.
                ForgetWithLogging(Initialize());
                return EmptyTags;
            }
            else if (file?.InlineCommentThreads != null)
            {
                var result = new List<ITagSpan<InlineTag>>();
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

                        if ((side == DiffSide.Left && thread.DiffLineType == DiffChangeType.Delete) ||
                            (side == DiffSide.Right && thread.DiffLineType != DiffChangeType.Delete))
                        {
                            linesWithComments[thread.LineNumber - startLine] = true;

                            result.Add(new TagSpan<ShowInlineTag>(
                                new SnapshotSpan(line.Start, line.End),
                                new ShowInlineTag(currentSession, thread)));
                        }
                    }

                    foreach (var chunk in file.Diff)
                    {
                        foreach (var line in chunk.Lines)
                        {
                            var lineNumber = (side == DiffSide.Left ? line.OldLineNumber : line.NewLineNumber) - 1;

                            if (lineNumber >= startLine &&
                                lineNumber <= endLine &&
                                !linesWithComments[lineNumber - startLine]
                                && (side == DiffSide.Right || line.Type == DiffChangeType.Delete))
                            {
                                var snapshotLine = span.Snapshot.GetLineFromLineNumber(lineNumber);
                                result.Add(new TagSpan<InlineTag>(
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

        async Task Initialize()
        {
            needsInitialize = false;

            var bufferInfo = sessionManager.GetTextBufferInfo(buffer);

            if (bufferInfo != null)
            {
                var commitSha = bufferInfo.Side == DiffSide.Left ? "HEAD" : bufferInfo.CommitSha;
                session = bufferInfo.Session;
                relativePath = bufferInfo.RelativePath;
                file = await session.GetFile(relativePath, commitSha);
                fileSubscription = file.LinesChanged.Subscribe(LinesChanged);
                side = bufferInfo.Side ?? DiffSide.Right;
                NotifyTagsChanged();
            }
            else
            {
                side = DiffSide.Right;
                await InitializeLiveFile();
                sessionManagerSubscription = sessionManager
                    .WhenAnyValue(x => x.CurrentSession)
                    .Skip(1)
                    .Subscribe(_ => ForgetWithLogging(InitializeLiveFile()));
            }
        }

        async Task InitializeLiveFile()
        {
            fileSubscription?.Dispose();
            fileSubscription = null;

            relativePath = sessionManager.GetRelativePath(buffer);

            if (relativePath != null)
            {
                file = await sessionManager.GetLiveFile(relativePath, view, buffer);

                var options = view.Options;
                visibleSubscription =
                    Observable.FromEventPattern<EditorOptionChangedEventArgs>(options, nameof(options.OptionChanged))
                    .Select(_ => Unit.Default)
                    .StartWith(Unit.Default)
                    .Select(x => options.GetOptionValue(InlineCommentTextViewOptions.MarginVisibleId))
                    .DistinctUntilChanged()
                    .Subscribe(VisibleChanged);
            }
            else
            {
                file = null;
            }

            NotifyTagsChanged();
        }

        void VisibleChanged(bool enabled)
        {
            if (enabled)
            {
                fileSubscription = fileSubscription ?? file.LinesChanged.Subscribe(LinesChanged);
            }
            else
            {
                fileSubscription?.Dispose();
                fileSubscription = null;
            }
        }

        static void ForgetWithLogging(Task task)
        {
            task.Catch(e => log.Error(e, "Exception caught while executing background task")).Forget();
        }

        void LinesChanged(IReadOnlyList<Tuple<int, DiffSide>> lines)
        {
            NotifyTagsChanged(lines.Where(x => x.Item2 == side).Select(x => x.Item1));
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
