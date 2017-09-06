using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using GitHub.Models;
using Microsoft.VisualStudio.Text;

namespace GitHub.InlineReviews.Models
{
    /// <summary>
    /// A file in a pull request session that tracks editor content.
    /// </summary>
    /// <remarks>
    /// A live session file extends <see cref="IPullRequestSessionFile"/> to update the file's
    /// review comments in real time, based on the contents of an editor and
    /// <see cref="IPullRequestSessionManager.CurrentSession"/>.
    /// </remarks>
    public sealed class PullRequestSessionLiveFile : PullRequestSessionFile, IPullRequestSessionLiveFile, IDisposable
    {
        readonly Subject<IReadOnlyList<int>> linesChanged = new Subject<IReadOnlyList<int>>();

        public PullRequestSessionLiveFile(
            string relativePath,
            ITextBuffer textBuffer,
            ISubject<ITextSnapshot, ITextSnapshot> rebuild)
            : base(relativePath)
        {
            TextBuffer = textBuffer;
            Rebuild = rebuild;
        }

        /// <summary>
        /// Gets the VS text buffer that the file is associated with.
        /// </summary>
        public ITextBuffer TextBuffer { get; }

        /// <summary>
        /// Gets a resource to dispose.
        /// </summary>
        public IDisposable ToDispose { get; internal set; }

        /// <summary>
        /// Gets a dictionary mapping review comments to tracking points in the <see cref="TextBuffer"/>.
        /// </summary>
        public IDictionary<IInlineCommentThreadModel, ITrackingPoint> TrackingPoints { get; internal set; }

        /// <summary>
        /// Gets an observable raised when the review comments for the file should be rebuilt.
        /// </summary>
        public ISubject<ITextSnapshot, ITextSnapshot> Rebuild { get; }

        /// <inheritdoc/>
        public IObservable<IReadOnlyList<int>> LinesChanged => linesChanged;

        /// <inheritdoc/>
        public override IReadOnlyList<IInlineCommentThreadModel> InlineCommentThreads
        {
            get { return base.InlineCommentThreads; }
            internal set
            {
                var lines = base.InlineCommentThreads?
                    .Concat(value ?? Enumerable.Empty<IInlineCommentThreadModel>())
                    .Select(x => x.LineNumber)
                    .Where(x => x >= 0)
                    .Distinct()
                    .ToList();
                base.InlineCommentThreads = value;
                NotifyLinesChanged(lines);
            }
        }

        /// <summary>
        /// Disposes of the resources in <see cref="ToDispose"/>.
        /// </summary>
        public void Dispose()
        {
            ToDispose?.Dispose();
            ToDispose = null;
        }

        /// <summary>
        /// Raises the <see cref="LinesChanged"/> signal.
        /// </summary>
        /// <param name="lines">The lines that have changed.</param>
        public void NotifyLinesChanged(IReadOnlyList<int> lines) => linesChanged.OnNext(lines);
    }
}
