using System;
using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Represents a view model that displays a pull request review.
    /// </summary>
    public interface IPullRequestReviewViewModel : IViewModel
    {
        /// <summary>
        /// Gets the local repository.
        /// </summary>
        ILocalRepositoryModel LocalRepository { get; }

        /// <summary>
        /// Gets the owner of the remote repository that contains the pull request.
        /// </summary>
        /// <remarks>
        /// The remote repository may be different from the local repository if the local
        /// repository is a fork and the user is viewing pull requests from the parent repository.
        /// </remarks>
        string RemoteRepositoryOwner { get; }

        /// <summary>
        /// Gets the underlying pull request review model.
        /// </summary>
        IPullRequestReviewModel Model { get; }

        /// <summary>
        /// Gets the underlying pull request model.
        /// </summary>
        IPullRequestModel PullRequestModel { get; }

        /// <summary>
        /// Gets the body of the review.
        /// </summary>
        string Body { get; }

        /// <summary>
        /// Gets the state of the pull request review as a string.
        /// </summary>
        string StateDisplay { get; }

        /// <summary>
        /// Gets a value indicating whether the pull request review should initially be expanded.
        /// </summary>
        bool IsExpanded { get; }

        /// <summary>
        /// Gets a value indicating whether the pull request review has a body or file comments.
        /// </summary>
        bool HasDetails { get; }

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