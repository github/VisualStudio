using System;
using GitHub.Models;
using System.Collections.ObjectModel;

namespace GitHub.ViewModels.Dialog
{
    /// <summary>
    /// ViewModel for the the Clone Repository dialog
    /// </summary>
    public interface IRepositoryCloneViewModel : IDialogContentViewModel, IConnectionInitializedViewModel
    {
        /// <summary>
        /// The list of repositories the current user may clone from the specified host.
        /// </summary>
        ObservableCollection<IRemoteRepositoryModel> Repositories { get; }

        bool FilterTextIsEnabled { get; }

        /// <summary>
        /// If true, then we failed to load the repositories.
        /// </summary>
        bool LoadingFailed { get; }

        /// <summary>
        /// Set to true if no repositories were found.
        /// </summary>
        bool NoRepositoriesFound { get; }

        /// <summary>
        /// Set to true if a repository is selected.
        /// </summary>
        bool CanClone { get; }

        string FilterText { get; set; }
    }
}
