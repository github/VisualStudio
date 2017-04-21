using System;
using System.IO;
using GitHub.Models;
using NullGuard;

namespace GitHub.ViewModels
{
    /// <summary>
    /// A file node in a pull request changes tree.
    /// </summary>
    public class PullRequestFileNode : IPullRequestFileNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestFileNode"/> class.
        /// </summary>
        /// <param name="repositoryPath">The absolute path to the repository.</param>
        /// <param name="path">The path to the file, relative to the repository.</param>
        /// <param name="sha">The SHA of the file.</param>
        /// <param name="status">The way the file was changed.</param>
        /// <param name="statusDisplay">The string to display in the [message] box next to the filename.</param>
        /// <param name="commentCount">The number of review comments on the file.</param>
        public PullRequestFileNode(
            string repositoryPath,
            string path,
            string sha,
            PullRequestFileStatus status,
            [AllowNull] string statusDisplay,
            int commentCount)
        {
            FileName = Path.GetFileName(path);
            DirectoryPath = Path.GetDirectoryName(path);
            DisplayPath = Path.Combine(Path.GetFileName(repositoryPath), DirectoryPath);
            Sha = sha;
            Status = status;
            StatusDisplay = statusDisplay;
            CommentCount = commentCount;
        }

        /// <summary>
        /// Gets the name of the file without path information.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the path to the file's directory, relative to the root of the repository.
        /// </summary>
        public string DirectoryPath { get; }

        /// <summary>
        /// Gets the path to display in the "Path" column of the changed files list.
        /// </summary>
        public string DisplayPath { get; }

        /// <summary>
        /// Gets the SHA of the file.
        /// </summary>
        public string Sha { get; }

        /// <summary>
        /// Gets the type of change that was made to the file.
        /// </summary>
        public PullRequestFileStatus Status { get; }

        /// <summary>
        /// Gets the string to display in the [message] box next to the filename.
        /// </summary>
        public string StatusDisplay { [return: AllowNull] get; }

        /// <summary>
        /// Gets the number of review comments on the file.
        /// </summary>
        public int CommentCount { get; }
    }
}
