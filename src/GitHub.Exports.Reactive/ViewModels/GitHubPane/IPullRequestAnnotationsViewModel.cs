using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IPullRequestAnnotationsViewModel : IPanePageViewModel
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
        /// Gets the number of the pull request.
        /// </summary>
        int PullRequestNumber { get; }

        int CheckRunId { get; }
        ReactiveCommand<object> NavigateToPullRequest { get; }
        string PullRequestTitle { get; }
        string CheckRunName { get; }
        IReadOnlyList<IPullRequestAnnotationItemViewModel> Annotations { get; }

        Task InitializeAsync(
            ILocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int pullRequestNumber,
            int checkRunId);
    }
}