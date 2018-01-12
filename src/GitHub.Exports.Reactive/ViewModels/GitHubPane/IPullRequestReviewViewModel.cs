using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Represents a view model for displaying details of a pull request review.
    /// </summary>
    public interface IPullRequestReviewViewModel : IPanePageViewModel, IDisposable
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
        /// Gets the Pull Request number.
        /// </summary>
        int PullRequestNumber { get; }

        /// <summary>
        /// Gets the ID of the pull request review.
        /// </summary>
        long PullRequestReviewId { get; }

        /// <summary>
        /// Gets the underlying pull request review model.
        /// </summary>
        IPullRequestReviewModel Model { get; }

        /// <summary>
        /// Gets the state of the pull request review as a string.
        /// </summary>
        string State { get; }

        /// <summary>
        /// Gets a value indicating whether the review is in the pending state.
        /// </summary>
        bool IsPending { get; }

        /// <summary>
        /// Gets the body of the pull request review.
        /// </summary>
        string Body { get; }

        /// <summary>
        /// Gets the pull request's changed files.
        /// </summary>
        IPullRequestFilesViewModel Files { get; }

        /// <summary>
        /// Gets a list of the file comments in the review.
        /// </summary>
        IReadOnlyList<IPullRequestReviewCommentModel> FileComments { get; }

        /// <summary>
        /// Gets a list of the outdated file comments in the review.
        /// </summary>
        IReadOnlyList<IPullRequestReviewCommentModel> OutdatedFileComments { get; }

        /// <summary>
        /// Gets the count of <see cref="FileComments"/> plus <see cref="OutdatedFileComments"/>.
        /// </summary>
        int CommentCount { get; }

        /// <summary>
        /// Gets a command which opens an inline comment in a diff view.
        /// </summary>
        ReactiveCommand<Unit> OpenComment { get; }

        /// <summary>
        /// Gets a command which navigates to the parent pull request.
        /// </summary>
        ReactiveCommand<object> NavigateToPullRequest { get; }

        /// <summary>
        /// Gets a command which submits the review.
        /// </summary>
        ReactiveCommand<Unit> Submit { get; }

        /// <summary>
        /// Initializes the view model with an existing review.
        /// </summary>
        /// <param name="localRepository">The local repository.</param>
        /// <param name="connection">The connection to the repository host.</param>
        /// <param name="owner">The pull request's repository owner.</param>
        /// <param name="repo">The pull request's repository name.</param>
        /// <param name="pullRequestNumber">The pull request number.</param>
        /// <param name="pullRequestReviewId">The pull request review ID.</param>
        Task InitializeAsync(
            ILocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int pullRequestNumber,
            long pullRequestReviewId);

        /// <summary>
        /// Initializes the view model for creating a new review.
        /// </summary>
        /// <param name="localRepository">The local repository.</param>
        /// <param name="connection">The connection to the repository host.</param>
        /// <param name="owner">The pull request's repository owner.</param>
        /// <param name="repo">The pull request's repository name.</param>
        /// <param name="pullRequestNumber">The pull request number.</param>
        Task InitializeNewAsync(
            ILocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int pullRequestNumber);

        /// <summary>
        /// Updates the data from a pull request model.
        /// </summary>
        /// <param name="pullRequest">The pull request model.</param>
        /// <returns>A task tracking the operation.</returns>
        Task Load(IPullRequestModel pullRequest);
    }
}
