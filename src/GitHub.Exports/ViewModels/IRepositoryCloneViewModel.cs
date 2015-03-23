using System.Collections.Generic;
using System.Windows.Input;
using GitHub.Models;
using System;

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
        ICommand CloneCommand { get; }
        IObservable<object> IsCloned { get; }

        /// <summary>
        /// The list of repositories the current user may clone from the specified host.
        /// </summary>
        ICollection<IRepositoryModel> Repositories { get; }

        IRepositoryModel SelectedRepository { get; set; }
    }
}
