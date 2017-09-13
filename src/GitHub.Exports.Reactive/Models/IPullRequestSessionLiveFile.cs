using System;
using System.Collections.Generic;
using GitHub.Services;

namespace GitHub.Models
{
    /// <summary>
    /// A file in a pull request session that tracks editor content.
    /// </summary>
    /// <remarks>
    /// A live session file extends <see cref="IPullRequestSessionFile"/> to update the file's
    /// review comments in real time, based on the contents of an editor and
    /// <see cref="IPullRequestSessionManager.CurrentSession"/>.
    /// </remarks>
    public interface IPullRequestSessionLiveFile : IPullRequestSessionFile
    {
        /// <summary>
        /// Gets an observable that is raised with a collection of 0-based line numbers when the
        /// review comments on the file are changed.
        /// </summary>
        IObservable<IReadOnlyList<int>> LinesChanged { get; }
    }
}
