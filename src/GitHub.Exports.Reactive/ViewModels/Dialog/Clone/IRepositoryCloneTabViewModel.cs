using System;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.ViewModels.Dialog.Clone
{
    /// <summary>
    /// Represents a tab in the repository clone dialog.
    /// </summary>
    public interface IRepositoryCloneTabViewModel : IViewModel
    {
        /// <summary>
        /// Gets a value that indicates whether the tab is enabled.
        /// </summary>
        /// <remarks>
        /// A disabled tab will be hidden.
        /// </remarks>
        bool IsEnabled { get; }

        /// <summary>
        /// Gets the selected repository, or null if no repository has been selected.
        /// </summary>
        RepositoryModel Repository { get; }

        /// <summary>
        /// Activates the tab.
        /// </summary>
        /// <remarks>
        /// Will be called each time the tab is selected.
        /// </remarks>
        Task Activate();
    }
}
