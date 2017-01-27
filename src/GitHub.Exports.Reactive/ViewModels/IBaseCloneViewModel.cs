using System.Collections.Generic;
using System.Reactive;
using GitHub.Models;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace GitHub.ViewModels
{
    /// <summary>
    /// ViewModel for the the Clone Repository dialog
    /// </summary>
    public interface IBaseCloneViewModel : IViewModel, IRepositoryCreationTarget
    {
        /// <summary>
        /// Command to clone the currently selected repository.
        /// </summary>
        IReactiveCommand<Unit> CloneCommand { get; }

        IRepositoryModel SelectedRepository { get; set; }
    }
}
