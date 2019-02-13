using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;
using ReactiveUI.Legacy;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Holds immutable state relating to the <see cref="IPullRequestDetailViewModel.Checkout"/> command.
    /// </summary>
    public interface IPullRequestCheckoutState
    {
        /// <summary>
        /// Gets the message to display on the checkout button.
        /// </summary>
        string Caption { get; }

        /// <summary>
        /// Gets a value indicating whether checkout is available.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Gets the message to display as the checkout button's tooltip.
        /// </summary>
        string ToolTip { get; }
    }

    /// <summary>
    /// Holds immutable state relating to the <see cref="IPullRequestDetailViewModel.Pull"/> and
    /// <see cref="IPullRequestDetailViewModel.Push"/> commands.
    /// </summary>
    public interface IPullRequestUpdateState
    {
        /// <summary>
        /// Gets the number of commits that the current branch is ahead of the PR branch.
        /// </summary>
        int CommitsAhead { get; }

        /// <summary>
        /// Gets the number of commits that the current branch is behind the PR branch.
        /// </summary>
        int CommitsBehind { get; }

        /// <summary>
        /// Gets a value indicating whether the current branch is up to date.
        /// </summary>
        bool UpToDate { get; }

        /// <summary>
        /// Gets the message to display when a pull cannot be carried out.
        /// </summary>
        string PullToolTip { get; }

        /// <summary>
        /// Gets the message to display when a push cannot be carried out.
        /// </summary>
        string PushToolTip { get; }
    }

    /// <summary>
    /// A view model which displays the details of a pull request.
    /// </summary>
    public interface IPullRequestDetailViewModel : IPanePageViewModel, IOpenInBrowser
    {
        /// <summary>
        /// Gets the underlying pull request model.
        /// </summary>
        PullRequestDetailModel Model { get; }

        /// <summary>
        /// Gets the session for the pull request.
        /// </summary>
        IPullRequestSession Session { get; }

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
        /// Gets the Pull Request number.
        /// </summary>
        int Number { get; }

        /// <summary>
        /// Gets the Pull Request author.
        /// </summary>
        IActorViewModel Author { get; }

        /// <summary>
        /// Gets a string describing how to display the pull request's source branch.
        /// </summary>
        string SourceBranchDisplayName { get; }

        /// <summary>
        /// Gets a string describing how to display the pull request's target branch.
        /// </summary>
        string TargetBranchDisplayName { get; }

        /// <summary>
        /// Gets a value indicating whether the pull request branch is checked out.
        /// </summary>
        bool IsCheckedOut { get; }

        /// <summary>
        /// Gets a value indicating whether the pull request comes from a fork.
        /// </summary>
        bool IsFromFork { get; }

        /// <summary>
        /// Gets the pull request body.
        /// </summary>
        string Body { get; }

        /// <summary>
        /// Gets the latest pull request review for each user.
        /// </summary>
        IReadOnlyList<IPullRequestReviewSummaryViewModel> Reviews { get; }

        /// <summary>
        /// Gets the pull request's changed files.
        /// </summary>
        IPullRequestFilesViewModel Files { get; }

        /// <summary>
        /// Gets the state associated with the <see cref="Checkout"/> command.
        /// </summary>
        IPullRequestCheckoutState CheckoutState { get; }

        /// <summary>
        /// Gets the state associated with the <see cref="Pull"/> and <see cref="Push"/> commands.
        /// </summary>
        IPullRequestUpdateState UpdateState { get; }

        /// <summary>
        /// Gets the error message to be displayed below the checkout button.
        /// </summary>
        string OperationError { get; }

        /// <summary>
        /// Gets a command that checks out the pull request locally.
        /// </summary>
        ReactiveCommand<Unit, Unit> Checkout { get; }

        /// <summary>
        /// Gets a command that pulls changes to the current branch.
        /// </summary>
        ReactiveCommand<Unit, Unit> Pull { get; }

        /// <summary>
        /// Gets a command that pushes changes from the current branch.
        /// </summary>
        ReactiveCommand<Unit, Unit> Push { get; }

        /// <summary>
        /// Sync submodules for PR branch.
        /// </summary>
        ReactiveCommand<Unit, Unit> SyncSubmodules { get; }

        /// <summary>
        /// Gets a command that opens the pull request on GitHub.
        /// </summary>
        ReactiveCommand<Unit, Unit> OpenOnGitHub { get; }

        /// <summary>
        /// Gets a command that navigates to a pull request review.
        /// </summary>
        ReactiveCommand<IPullRequestReviewSummaryViewModel, Unit> ShowReview { get; }

        /// <summary>
        /// Gets a command that navigates to a pull request's check run annotation list.
        /// </summary>
        ReactiveCommand<IPullRequestCheckViewModel, Unit> ShowAnnotations { get; }

        /// <summary>
        /// Gets the latest pull request checks & statuses.
        /// </summary>
        IReadOnlyList<IPullRequestCheckViewModel> Checks { get; }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <param name="localRepository">The local repository.</param>
        /// <param name="connection">The connection to the repository host.</param>
        /// <param name="owner">The pull request's repository owner.</param>
        /// <param name="repo">The pull request's repository name.</param>
        /// <param name="number">The pull request number.</param>
        Task InitializeAsync(
            LocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int number);

        /// <summary>
        /// Gets the full path to a file in the working directory.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>The full path to the file in the working directory.</returns>
        string GetLocalFilePath(IPullRequestFileNode file);
    }
}
