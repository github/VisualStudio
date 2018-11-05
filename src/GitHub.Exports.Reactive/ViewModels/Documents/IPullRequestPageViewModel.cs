using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// View model for displaying a pull request in a document window.
    /// </summary>
    public interface IPullRequestPageViewModel : IPullRequestViewModelBase
    {
        /// <summary>
        /// Gets the number of commits in the pull request.
        /// </summary>
        int CommitCount { get; }

        /// <summary>
        /// Gets the pull request's timeline.
        /// </summary>
        IReadOnlyList<IViewModel> Timeline { get; }

        /// <summary>
        /// Gets a command that will open a commit in Team Explorer.
        /// </summary>
        ReactiveCommand<string, Unit> ShowCommit { get; }

        /// <summary>
        /// Initializes the view model with data.
        /// </summary>
        /// <param name="repository">The repository to which the pull request belongs.</param>
        /// <param name="localRepository">The local repository.</param>
        /// <param name="currentUser">The currently logged in user.</param>
        /// <param name="model">The pull request model.</param>
        Task InitializeAsync(
            IRemoteRepositoryModel repository,
            ILocalRepositoryModel localRepository,
            ActorModel currentUser,
            PullRequestDetailModel model);
    }
}