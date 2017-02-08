using System.Collections.Generic;
using System.Reactive;
using GitHub.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using GitHub.Collections;

namespace GitHub.ViewModels
{
    /// <summary>
    /// ViewModel for the the Clone Repository dialog
    /// </summary>
    public interface IRepositoryCloneViewModel : IBaseCloneViewModel, IRepositoryCreationTarget
    {

        /// <summary>
        /// The list of repositories the current user may clone from the specified host.
        /// </summary>
        TrackingCollection<IRemoteRepositoryModel> Repositories { get; }

        bool FilterTextIsEnabled { get; }

        /// <summary>
        /// Whether or not we are currently loading repositories.
        /// </summary>
        bool IsLoading { get; }

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
