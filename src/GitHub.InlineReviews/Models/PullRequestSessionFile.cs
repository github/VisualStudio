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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "linesChanged is shared and shouldn't be disposed")]
    public class PullRequestSessionFile : ReactiveObject, IPullRequestSessionFile
    {
        readonly Subject<IReadOnlyList<Tuple<int, DiffSide>>> linesChanged = new Subject<IReadOnlyList<Tuple<int, DiffSide>>>();
        IReadOnlyList<DiffChunk> diff;
        string commitSha;
        IReadOnlyList<IInlineCommentThreadModel> inlineCommentThreads;
        IReadOnlyList<InlineAnnotationModel> inlineAnnotations;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestSessionFile"/> class.
        /// </summary>
        /// <param name="relativePath">
        /// The relative path to the file in the repository.
        /// </param>
        /// <param name="commitSha">
        /// The commit to pin the file to, or "HEAD" to follow the pull request head.
        /// </param>
        public PullRequestSessionFile(string relativePath, string commitSha = "HEAD")
        {
            RelativePath = relativePath;
            this.commitSha = commitSha;
            IsTrackingHead = commitSha == "HEAD";
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
            internal set
            {
                if (value != commitSha)
                {
                    if (!IsTrackingHead)
                    {
                        throw new GitHubLogicException(
                            "Cannot change the CommitSha of a PullRequestSessionFile that is not tracking HEAD.");
                    }

                    this.RaiseAndSetIfChanged(ref commitSha, value);
                }
            }
        }

        /// <inheritdoc/>
        public bool IsTrackingHead { get; }

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
                this.RaisePropertyChanging();
                inlineCommentThreads = value;
                this.RaisePropertyChanged();
                NotifyLinesChanged(lines);
            }
        }

        /// <inheritdoc/>
        public IObservable<IReadOnlyList<Tuple<int, DiffSide>>> LinesChanged => linesChanged;

        /// <inheritdoc/>
        public IReadOnlyList<InlineAnnotationModel> InlineAnnotations
        {
            get
            {
                return inlineAnnotations;
            }
            set
            {
                var lines = (inlineAnnotations ?? Enumerable.Empty<InlineAnnotationModel>())?
                    .Concat(value ?? Enumerable.Empty<InlineAnnotationModel>())
                    .Select(x => Tuple.Create(x.StartLine, DiffSide.Right))
                    .Where(x => x.Item1 >= 0)
                    .Distinct()
                    .ToList();

                this.RaisePropertyChanging();
                inlineAnnotations = value;
                this.RaisePropertyChanged();
                NotifyLinesChanged(lines);
            }
        }

        /// <summary>
        /// Raises the <see cref="LinesChanged"/> signal.
        /// </summary>
        /// <param name="lines">The lines that have changed.</param>
        public void NotifyLinesChanged(IReadOnlyList<Tuple<int, DiffSide>> lines) => linesChanged.OnNext(lines);
    }
}
