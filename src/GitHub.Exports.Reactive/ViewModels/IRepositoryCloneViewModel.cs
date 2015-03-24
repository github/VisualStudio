using System.Collections.Generic;
using System.Windows.Input;
using GitHub.Models;
using System;
using ReactiveUI;
using System.Reactive;

namespace GitHub.ViewModels
{
    /// <summary>
    /// ViewModel for the the Clone Repository dialog
    /// </summary>
    public interface IRepositoryCloneViewModel : IViewModel
    {
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
        string FilterText { get; set; }
    }
}
