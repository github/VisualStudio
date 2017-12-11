using System;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Represents a file or directory node in a pull request changes tree.
    /// </summary>
    public interface IPullRequestChangeNode
    {
        /// <summary>
        /// Gets the path to the file (not including the filename) or directory, relative to the
        /// root of the repository.
        /// </summary>
        string DirectoryPath { get; }
    }
}