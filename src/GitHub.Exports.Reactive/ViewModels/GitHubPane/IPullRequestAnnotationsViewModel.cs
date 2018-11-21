using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;
using ReactiveUI.Legacy;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// A viewmodel which displays a list of annotations for a pull request's check run.
    /// </summary>
    public interface IPullRequestAnnotationsViewModel : IPanePageViewModel
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
        /// Gets the title of the pull request.
        /// </summary>
        string PullRequestTitle { get; }

        /// <summary>
        /// Gets the id of the check run.
        /// </summary>
        string CheckRunId { get; }

        /// <summary>
        /// Gets the name of the check run.
        /// </summary>
        string CheckRunName { get; }

        /// <summary>
        /// Gets a command which navigates to the parent pull request.
        /// </summary>
        ReactiveCommand<Unit, Unit> NavigateToPullRequest { get; }

        /// <summary>
        /// Name of the Check Suite.
        /// </summary>
        string CheckSuiteName { get; }

        /// <summary>
        /// Summary of the Check Run
        /// </summary>
        string CheckRunSummary { get; }

        /// <summary>
        /// Text of the Check Run
        /// </summary>
        string CheckRunText { get; }

        /// <summary>
        /// Gets a dictionary of annotations by file path.
        /// </summary>
        IReadOnlyDictionary<string, IPullRequestAnnotationItemViewModel[]> AnnotationsDictionary { get; }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <param name="localRepository">The local repository.</param>
        /// <param name="connection">The connection to the repository host.</param>
        /// <param name="owner">The pull request's repository owner.</param>
        /// <param name="repo">The pull request's repository name.</param>
        /// <param name="pullRequestNumber">The pull request's number.</param>
        /// <param name="checkRunId">The pull request's check run id.</param>
        Task InitializeAsync(
            LocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int pullRequestNumber,
            string checkRunId);
    }
}