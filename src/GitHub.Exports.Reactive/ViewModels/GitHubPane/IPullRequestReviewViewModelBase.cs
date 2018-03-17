using System;
using System.Reactive;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Base interface for view model that display pull request reviews.
    /// </summary>
    public interface IPullRequestReviewViewModelBase : IViewModel
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
    }
}
