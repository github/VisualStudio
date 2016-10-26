using System;
using GitHub.Models;

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
        /// <param name="path">The path to the file, relative to the repository.</param>
        /// <param name="changeType">The way the file was changed.</param>
        public PullRequestFileNode(string path, PullRequestFileStatus status)
        {
            FileName = System.IO.Path.GetFileName(path);
            Path = System.IO.Path.GetDirectoryName(path);
            Status = status;
        }

        /// <summary>
        /// Gets the name of the file without path information.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the path to the file (not including the filename), relative to the root of the repository.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the type of change that was made to the file.
        /// </summary>
        public PullRequestFileStatus Status { get; }
    }
}
