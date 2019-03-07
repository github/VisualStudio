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
        int annotationNoticeCount;
        int annotationWarningCount;
        int _annotationFailureCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestFileNode"/> class.
        /// </summary>
        /// <param name="repositoryPath">The absolute path to the repository.</param>
        /// <param name="relativePath">The path to the file, relative to the repository.</param>
        /// <param name="sha">The SHA of the file.</param>
        /// <param name="status">The way the file was changed.</param>
        /// <param name="statusDisplay">The string to display in the [message] box next to the filename.</param>
        /// <param name="oldPath">
        /// The old path of a moved/renamed file, relative to the repository. Should be null if the
        /// file was not moved/renamed.
        /// </param>
        public PullRequestFileNode(
            string repositoryPath,
            string relativePath,
            string sha,
            PullRequestFileStatus status,
            string oldPath)
        {
            Guard.ArgumentNotEmptyString(repositoryPath, nameof(repositoryPath));
            Guard.ArgumentNotEmptyString(relativePath, nameof(relativePath));
            Guard.ArgumentNotEmptyString(sha, nameof(sha));

            FileName = Path.GetFileName(relativePath);
            RelativePath = relativePath.Replace("/", "\\");
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
                    StatusDisplay = Path.GetDirectoryName(oldPath) == Path.GetDirectoryName(relativePath) ?
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
        /// Gets the path to the file, relative to the root of the repository.
        /// </summary>
        public string RelativePath { get; }

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

        /// <summary>
        /// Gets or sets the number of annotation notices on the file.
        /// </summary>
        public int AnnotationNoticeCount
        {
            get { return annotationNoticeCount; }
            set { this.RaiseAndSetIfChanged(ref annotationNoticeCount, value); }
        }

        /// <summary>
        /// Gets or sets the number of annotation errors on the file.
        /// </summary>
        public int AnnotationWarningCount
        {
            get { return annotationWarningCount; }
            set { this.RaiseAndSetIfChanged(ref annotationWarningCount, value); }
        }

        /// <summary>
        /// Gets or sets the number of annotation failures on the file.
        /// </summary>
        public int AnnotationFailureCount
        {
            get { return _annotationFailureCount; }
            set { this.RaiseAndSetIfChanged(ref _annotationFailureCount, value); }
        }
    }
}
