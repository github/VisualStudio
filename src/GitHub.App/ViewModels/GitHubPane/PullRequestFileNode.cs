using System;
using System.IO;
using GitHub.App;
using GitHub.Extensions;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// A file node in a pull request changes tree.
    /// </summary>
    public class PullRequestFileNode : ReactiveObject, IPullRequestFileNode
    {
        int commentCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestFileNode"/> class.
        /// </summary>
        /// <param name="repositoryPath">The absolute path to the repository.</param>
        /// <param name="path">The path to the file, relative to the repository.</param>
        /// <param name="sha">The SHA of the file.</param>
        /// <param name="status">The way the file was changed.</param>
        /// <param name="statusDisplay">The string to display in the [message] box next to the filename.</param>
        /// <param name="oldPath">
        /// The old path of a moved/renamed file, relative to the repository. Should be null if the
        /// file was not moved/renamed.
        /// </param>
        public PullRequestFileNode(
            string repositoryPath,
            string path,
            string sha,
            PullRequestFileStatus status,
            string oldPath)
        {
            Guard.ArgumentNotEmptyString(repositoryPath, nameof(repositoryPath));
            Guard.ArgumentNotEmptyString(path, nameof(path));
            Guard.ArgumentNotEmptyString(sha, nameof(sha));

            FileName = Path.GetFileName(path);
            DirectoryPath = Path.GetDirectoryName(path);
            Sha = sha;
            Status = status;
            OldPath = oldPath;

            if (status == PullRequestFileStatus.Added)
            {
                StatusDisplay = Resources.AddedFileStatus;
            }
            else if (status == PullRequestFileStatus.Renamed)
            {
                if (oldPath != null)
                {
                    StatusDisplay = Path.GetDirectoryName(oldPath) == Path.GetDirectoryName(path) ?
                            Path.GetFileName(oldPath) : oldPath;
                }
                else
                {
                    StatusDisplay = Resources.RenamedFileStatus;
                }
            }
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
        /// Gets the old path of a moved/renamed file, relative to the root of the repository.
        /// </summary>
        public string OldPath { get; }

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
        public string StatusDisplay { get; }

        /// <summary>
        /// Gets or sets the number of review comments on the file.
        /// </summary>
        public int CommentCount
        {
            get { return commentCount; }
            set { this.RaiseAndSetIfChanged(ref commentCount, value); }
        }
    }
}
