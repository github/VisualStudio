using GitHub.Models;

namespace GitHub.ViewModels
{
    public interface IPullRequestFileNode : IPullRequestChangeNode
    {
        /// <summary>
        /// Gets the name of the file without path information.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Gets the path to display in the "Path" column of the changed files list.
        /// </summary>
        string DisplayPath { get; }

        /// <summary>
        /// Gets the type of change that was made to the file.
        /// </summary>
        PullRequestFileStatus Status { get; }
    }
}