using System.Collections.Generic;
using System.Reactive;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// ViewModel for the the Clone Repository dialog
    /// </summary>
    public interface IRepositoryCloneViewModel : IViewModel
    {
        /// <summary>
        /// Command to load the repositories.
        /// </summary>
        IReactiveCommand<IReadOnlyList<IRepositoryModel>> LoadRepositoriesCommand { get; }

        /// <summary>
        /// Command to clone the currently selected repository.
        /// </summary>
        IReactiveCommand<Unit> CloneCommand { get; }

        /// <summary>
        /// The list of repositories the current user may clone from the specified host.
        /// </summary>
        IReactiveDerivedList<IRepositoryModel> FilteredRepositories { get; }

        IRepositoryModel SelectedRepository { get; set; }

        bool FilterTextIsEnabled { get; }

        /// <summary>
        /// Whether or not we are currently loading repositories.
        /// </summary>
        bool IsLoading { get; }

        /// <summary>
        /// If true, then we failed to load the repositories.
        /// </summary>
        bool LoadingFailed { get; }

        string FilterText { get; set; }
    }
}
