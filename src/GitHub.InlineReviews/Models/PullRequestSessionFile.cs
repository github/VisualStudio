using System;
using System.Collections.Generic;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.InlineReviews.Models
{
    /// <summary>
    /// A file in a pull request session.
    /// </summary>
    /// <remarks>
    /// A pull request session file represents the real-time state of a file in a pull request in
    /// the IDE. It represents the state of a file from the pull request model updated to the 
    /// current state of the code on disk and in the editor.
    /// </remarks>
    class PullRequestSessionFile : ReactiveObject, IPullRequestSessionFile
    {
        IList<DiffChunk> diff;
        string relativePath;

        /// <inheritdoc/>
        public string RelativePath
        {
            get { return relativePath; }
            internal set { this.RaiseAndSetIfChanged(ref relativePath, value); }
        }

        /// <inheritdoc/>
        public IList<DiffChunk> Diff
        {
            get { return diff; }
            internal set { this.RaiseAndSetIfChanged(ref diff, value); }
        }

        /// <inheritdoc/>
        public string BaseCommit { get; internal set; }

        /// <inheritdoc/>
        public string BaseSha { get; internal set; }

        /// <inheritdoc/>
        public string CommitSha { get; internal set; }

        /// <inheritdoc/>
        public IReactiveList<IInlineCommentThreadModel> InlineCommentThreads { get; }
            = new ReactiveList<IInlineCommentThreadModel>();
    }
}
