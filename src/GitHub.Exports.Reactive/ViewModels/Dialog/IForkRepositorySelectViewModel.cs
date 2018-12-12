using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    /// <summary>
    /// View model for selecting the account to fork a repository to.
    /// </summary>
    public interface IForkRepositorySelectViewModel : IDialogContentViewModel
    {
        /// <summary>
        /// Gets a list of accounts that the repository can be forked to.
        /// </summary>
        IReadOnlyList<IAccount> Accounts { get; }

        /// <summary>
        /// Gets a list of existing forks for accounts that the owner has access to.
        /// </summary>
        IReadOnlyList<RemoteRepositoryModel> ExistingForks { get; }

        /// <summary>
        /// Gets a value indicating whether the view model is loading.
        /// </summary>
        bool IsLoading { get; }

        /// <summary>
        /// Gets a command that is executed when the user selects an item in <see cref="Accounts"/>.
        /// </summary>
        ReactiveCommand<IAccount, IAccount> SelectedAccount { get; }

        /// <summary>
        /// Gets a command that is executed when the user selects an item in <see cref="ExistingForks"/>.
        /// </summary>
        ReactiveCommand<RemoteRepositoryModel, Unit> SwitchOrigin { get; }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <param name="repository">The repository to fork.</param>
        /// <param name="connection">The connection to use.</param>
        Task InitializeAsync(LocalRepositoryModel repository, IConnection connection);
    }
}
