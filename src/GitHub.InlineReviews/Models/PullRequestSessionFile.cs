using System;
using System.Collections.Generic;
using GitHub.Models;
using ReactiveUI;
using GitHub.InlineReviews.Services;

namespace GitHub.InlineReviews.Models
{
    /// <summary>
    /// A file in a pull request session.
    /// </summary>
    /// <remarks>
    /// A pull request session file represents the real-time state of a file in a pull request in
    /// the IDE. If the pull request branch is checked out, it represents the state of a file from
    /// the pull request model updated to the current state of the code on disk and in the editor.
    /// </remarks>
    /// <seealso cref="PullRequestSession"/>
    /// <seealso cref="PullRequestSessionManager"/>
    class PullRequestSessionFile : ReactiveObject, IPullRequestSessionFile
    {
        IList<DiffChunk> diff;
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

        /// <inheritdoc/>
        public string RelativePath { get; }

        /// <inheritdoc/>
        public IList<DiffChunk> Diff
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
        public IEditorContentSource ContentSource { get; internal set; }

        /// <inheritdoc/>
        public IReadOnlyList<IInlineCommentThreadModel> InlineCommentThreads
        {
            get { return inlineCommentThreads; }
            internal set { this.RaiseAndSetIfChanged(ref inlineCommentThreads, value); }
        }
    }
}
