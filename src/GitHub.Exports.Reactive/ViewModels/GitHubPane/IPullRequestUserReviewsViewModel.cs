using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Displays all reviews made by a user on a pull request.
    /// </summary>
    public interface IPullRequestUserReviewsViewModel : IPanePageViewModel
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
        /// Gets the number of the pull request.
        /// </summary>
        int PullRequestNumber { get; }

        /// <summary>
        /// Gets the reviews made by the <see cref="User"/>.
        /// </summary>
        IReadOnlyList<IPullRequestReviewViewModel> Reviews { get; }

        /// <summary>
        /// Gets the title of the pull request.
        /// </summary>
        string PullRequestTitle { get; }

        /// <summary>
        /// Gets the user whose reviews are being shown.
        /// </summary>
        IActorViewModel User { get; }

        /// <summary>
        /// Gets a command that navigates to the parent pull request in the GitHub pane.
        /// </summary>
        ReactiveCommand<Unit, Unit> NavigateToPullRequest { get; }

        /// <summary>
        /// Initializes the view model, loading data from the API.
        /// </summary>
        /// <param name="localRepository">The local repository.</param>
        /// <param name="connection">The connection to the repository host.</param>
        /// <param name="owner">The pull request's repository owner.</param>
        /// <param name="repo">The pull request's repository name.</param>
        /// <param name="pullRequestNumber">The pull request number.</param>
        /// <param name="login">The user's login.</param>
        Task InitializeAsync(
            LocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int pullRequestNumber,
            string login);
    }
}