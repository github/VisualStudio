using System;
using System.Collections.Generic;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Represents a view model that displays a pull request review.
    /// </summary>
    public interface IPullRequestReviewViewModel : IPullRequestReviewViewModelBase
    {
        /// <summary>
        /// Gets the body of the review.
        /// </summary>
        string Body { get; }

        /// <summary>
        /// Gets the state of the pull request review as a string.
        /// </summary>
        string StateDisplay { get; }

        /// <summary>
        /// Gets a value indicating whether the pull request review is the latest by the author.
        /// </summary>
        bool IsLatest { get; }

        /// <summary>
        /// Gets a list of the file comments in the review.
        /// </summary>
        IReadOnlyList<IPullRequestReviewFileCommentViewModel> FileComments { get; }

        /// <summary>
        /// Gets a list of outdated file comments in the review.
        /// </summary>
        IReadOnlyList<IPullRequestReviewFileCommentViewModel> OutdatedFileComments { get; }
    }
}
