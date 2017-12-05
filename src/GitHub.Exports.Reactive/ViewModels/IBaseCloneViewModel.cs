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
    public interface IBaseCloneViewModel : IDialogViewModel, IRepositoryCreationTarget
    {
        /// <summary>
        /// Signals that the user clicked the clone button.
        /// </summary>
        IReactiveCommand<object> CloneCommand { get; }

        IRepositoryModel SelectedRepository { get; set; }
    }
}
