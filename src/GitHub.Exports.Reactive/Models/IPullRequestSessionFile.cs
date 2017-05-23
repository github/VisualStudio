using System;
using System.Collections.Generic;
using ReactiveUI;

namespace GitHub.Models
{
    /// <summary>
    /// A file in a pull request session.
    /// </summary>
    /// <remarks>
    /// A pull request session file represents the real-time state of a file in a pull request in
    /// the IDE. It represents the state of a file from the pull request model updated to the 
    /// current state of the code on disk and in the editor.
    /// </remarks>
    public interface IPullRequestSessionFile
    {
        /// <summary>
        /// Gets the SHA of the current commit of the file, or null if the file has uncommitted
        /// changes.
        /// </summary>
        string CommitSha { get; }

        /// <summary>
        /// Gets the path to the file relative to the repository.
        /// </summary>
        string RelativePath { get; }

        /// <summary>
        /// Gets the diff between the PR base and the current state of the file.
        /// </summary>
        IList<DiffChunk> Diff { get; }

        /// <summary>
        /// Gets the inline comments threads for the file.
        /// </summary>
        IReactiveList<IInlineCommentThreadModel> InlineCommentThreads { get; }
    }
}
