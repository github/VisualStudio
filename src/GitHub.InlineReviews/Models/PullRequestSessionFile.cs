using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.InlineReviews.Models
{
    /// <summary>
    /// A file in a pull request session.
    /// </summary>
    /// <remarks>
    /// A <see cref="PullRequestSessionFile"/> holds the review comments for a file in a pull
    /// request together with associated information such as the commit SHA of the file and the
    /// diff with the file's merge base.
    /// </remarks>
    /// <seealso cref="PullRequestSession"/>
    /// <seealso cref="PullRequestSessionManager"/>
    public class PullRequestSessionFile : ReactiveObject, IPullRequestSessionFile, IDisposable
    {
        readonly Subject<IReadOnlyList<Tuple<int, DiffSide>>> linesChanged = new Subject<IReadOnlyList<Tuple<int, DiffSide>>>();
        IReadOnlyList<DiffChunk> diff;
        string commitSha;
        IReadOnlyList<IInlineCommentThreadModel> inlineCommentThreads;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestSessionFile"/> class.
        /// </summary>
        /// <param name="relativePath">
        /// The relative path to the file in the repository.
        /// </param>
        public PullRequestSessionFile(string relativePath)
        {
            RelativePath = relativePath;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;

                if (disposing)
                {
                    linesChanged.Dispose();
                }
            }
        }

        /// <inheritdoc/>
        public string RelativePath { get; }

        /// <inheritdoc/>
        public IReadOnlyList<DiffChunk> Diff
        {
            get { return diff; }
            internal set { this.RaiseAndSetIfChanged(ref diff, value); }
        }

        /// <inheritdoc/>
        public string BaseSha { get; internal set; }

        /// <inheritdoc/>
        public string CommitSha
        {
            get { return commitSha; }
            internal set { this.RaiseAndSetIfChanged(ref commitSha, value); }
        }

        /// <inheritdoc/>
        public IReadOnlyList<IInlineCommentThreadModel> InlineCommentThreads
        {
            get { return inlineCommentThreads; }
            set
            {
                var lines = (inlineCommentThreads ?? Enumerable.Empty<IInlineCommentThreadModel>())?
                    .Concat(value ?? Enumerable.Empty<IInlineCommentThreadModel>())
                    .Select(x => Tuple.Create(x.LineNumber, x.DiffLineType == DiffChangeType.Delete ? DiffSide.Left : DiffSide.Right))
                    .Where(x => x.Item1 >= 0)
                    .Distinct()
                    .ToList();
                inlineCommentThreads = value;
                NotifyLinesChanged(lines);
            }
        }

        /// <inheritdoc/>
        public IObservable<IReadOnlyList<Tuple<int, DiffSide>>> LinesChanged => linesChanged;

        /// <summary>
        /// Raises the <see cref="LinesChanged"/> signal.
        /// </summary>
        /// <param name="lines">The lines that have changed.</param>
        public void NotifyLinesChanged(IReadOnlyList<Tuple<int, DiffSide>> lines) => linesChanged.OnNext(lines);
    }
}
