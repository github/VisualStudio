using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IPullRequestFileNode : IPullRequestChangeNode
    {
        /// <summary>
        /// Gets the name of the file without path information.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Gets the old path of a moved/renamed file, relative to the root of the repository.
        /// </summary>
        string OldPath { get; }

        /// <summary>
        /// Gets the SHA of the file.
        /// </summary>
        string Sha { get; }

        /// <summary>
        /// Gets the type of change that was made to the file.
        /// </summary>
        PullRequestFileStatus Status { get; }

        /// <summary>
        /// Gets the string to display in the [message] box next to the filename.
        /// </summary>
        string StatusDisplay { get; }

        /// <summary>
        /// Gets the number of review comments on the file.
        /// </summary>
        int CommentCount { get; }

        /// <summary>
        /// Gets or sets the number of annotation notices on the file.
        /// </summary>
        int AnnotationNoticeCount { get; }

        /// <summary>
        /// Gets or sets the number of annotation errors on the file.
        /// </summary>
        int AnnotationWarningCount { get; }

        /// <summary>
        /// Gets or sets the number of annotation failures on the file.
        /// </summary>
        int AnnotationFailureCount { get; }
    }
}