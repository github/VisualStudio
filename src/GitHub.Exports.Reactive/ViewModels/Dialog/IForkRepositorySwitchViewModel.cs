using System.Reactive;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    /// <summary>
    /// View model for selecting the fork to switch to
    /// </summary>
    public interface IForkRepositorySwitchViewModel : IDialogContentViewModel
    {
        RepositoryModel SourceRepository { get; }

        RepositoryModel DestinationRepository { get; }

        /// <summary>
        /// Gets a command that is executed when the user clicks the "Fork" button.
        /// </summary>
        ReactiveCommand<Unit, Unit> SwitchFork { get; }

        bool ResetMasterTracking { get; set; }

        bool AddUpstream { get; set; }

        bool UpdateOrigin { get; set; }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <param name="sourceRepository">The repository to fork.</param>
        /// <param name="remoteRepository"></param>
        void Initialize(LocalRepositoryModel sourceRepository, RemoteRepositoryModel remoteRepository);
    }
}