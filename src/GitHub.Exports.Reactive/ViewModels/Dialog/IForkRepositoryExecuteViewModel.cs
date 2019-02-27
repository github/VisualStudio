using System;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using Octokit;
using ReactiveUI;
using IConnection = GitHub.Models.IConnection;

namespace GitHub.ViewModels.Dialog
{
    /// <summary>
    /// View model for selecting the account to fork a repository to.
    /// </summary>
    public interface IForkRepositoryExecuteViewModel : IDialogContentViewModel
    {
        RepositoryModel SourceRepository { get; }

        RepositoryModel DestinationRepository { get; }

        IAccount DestinationAccount { get; }
      
        /// <summary>
        /// Gets a command that is executed when the user clicks the "Fork" button.
        /// </summary>
        ReactiveCommand<Unit, Repository> CreateFork { get; }

        ReactiveCommand<Unit, Unit> BackCommand { get; }

        bool ResetMasterTracking { get; set; }

        bool AddUpstream { get; set; }

        bool UpdateOrigin { get; set; }

        bool CanAddUpstream { get; }

        bool CanResetMasterTracking { get; }

        string Error { get; }
        IObservable<Unit> Back { get; }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <param name="sourceRepository">The repository to fork.</param>
        /// <param name="destinationAccount">The account to fork to.</param>
        /// <param name="connection">The connection to use.</param>
        Task InitializeAsync(
            LocalRepositoryModel sourceRepository, 
            IAccount destinationAccount, 
            IConnection connection);
    }
}
