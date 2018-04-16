using System;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    /// <summary>
    /// View model for selecting the account to fork a repository to.
    /// </summary>
    public interface IForkRepositoryExecuteViewModel : IDialogContentViewModel
    {
        IRepositoryModel SourceRepository { get; }

        IRepositoryModel DestinationRepository { get; }

        /// <summary>
        /// Gets a command that is executed when the user clicks the "Fork" button.
        /// </summary>
        ReactiveCommand<object> Start { get; }

        bool ResetMasterTracking { get; set; }

        bool AddUpstream { get; set; }

        bool UpdateOrigin { get; set; }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <param name="repository">The repository to fork.</param>
        /// <param name="connection">The connection to use.</param>
        /// <param name="account">The account to fork to.</param>
        Task InitializeAsync(
            ILocalRepositoryModel repository,
            IConnection connection,
            IAccount account);
    }
}
