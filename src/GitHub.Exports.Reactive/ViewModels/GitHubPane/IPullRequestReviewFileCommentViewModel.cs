using System;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Represents a view model for a file comment in an <see cref="IPullRequestReviewViewModel"/>.
    /// </summary>
    public interface IPullRequestReviewFileCommentViewModel
    {
        /// <summary>
        /// Gets the body of the comment.
        /// </summary>
        string Body { get; }

        /// <summary>
        /// Gets the line number of the comment or -1 for an outdated comment.
        /// </summary>
        int LineNumber { get; }

        /// <summary>
        /// Gets the path to the file, relative to the root of the repository.
        /// </summary>
        string RelativePath { get; }
    }
}
