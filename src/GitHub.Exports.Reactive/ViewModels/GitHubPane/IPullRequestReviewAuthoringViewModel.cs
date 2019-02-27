using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Represents a view model for displaying details of a pull request review that is being
    /// authored.
    /// </summary>
    public interface IPullRequestReviewAuthoringViewModel : IPanePageViewModel, IDisposable
    {
        /// <summary>
        /// Gets the local repository.
        /// </summary>
        LocalRepositoryModel LocalRepository { get; }

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
        PullRequestReviewModel Model { get; }

        /// <summary>
        /// Gets the underlying pull request model.
        /// </summary>
        PullRequestDetailModel PullRequestModel { get; }

        /// <summary>
        /// Gets or sets the body of the pull request review to be submitted.
        /// </summary>
        string Body { get; }

        /// <summary>
        /// Gets a value indicating whether the user can approve/request changes on the pull request.
        /// </summary>
        bool CanApproveRequestChanges { get; }

        /// <summary>
        /// Gets the pull request's changed files.
        /// </summary>
        IPullRequestFilesViewModel Files { get; }

        /// <summary>
        /// Gets a list of the file comments in the review.
        /// </summary>
        IReadOnlyList<IPullRequestReviewFileCommentViewModel> FileComments { get; }

        /// <summary>
        /// Gets the error message to be displayed in the action area as a result of an error submitting.
        /// </summary>
        string OperationError { get; }

        /// <summary>
        /// Gets a command which navigates to the parent pull request.
        /// </summary>
        ReactiveCommand<Unit, Unit> NavigateToPullRequest { get; }

        /// <summary>
        /// Gets a command which submits the review as an approval.
        /// </summary>
        ReactiveCommand<Unit, Unit> Approve { get; }

        /// <summary>
        /// Gets a command which submits the review as a comment.
        /// </summary>
        ReactiveCommand<Unit, Unit> Comment { get; }

        /// <summary>
        /// Gets a command which submits the review requesting changes.
        /// </summary>
        ReactiveCommand<Unit, Unit> RequestChanges { get; }

        /// <summary>
        /// Gets a command which cancels the review.
        /// </summary>
        ReactiveCommand<Unit, Unit> Cancel { get; }

        /// <summary>
        /// Initializes the view model for creating a new review.
        /// </summary>
        /// <param name="localRepository">The local repository.</param>
        /// <param name="connection">The connection to the repository host.</param>
        /// <param name="owner">The pull request's repository owner.</param>
        /// <param name="repo">The pull request's repository name.</param>
        /// <param name="pullRequestNumber">The pull request number.</param>
        Task InitializeAsync(
            LocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int pullRequestNumber);
    }
}
