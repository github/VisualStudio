using System;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.ViewModels.Dialog
{
    /// <summary>
    /// View model for forking a repository.
    /// </summary>
    public interface IForkRepositoryViewModel : IDialogContentViewModel
    {
        /// <summary>
        /// Gets the currently displayed page.
        /// </summary>
        IDialogContentViewModel Content { get; }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <param name="repository">The repository to fork.</param>
        /// <param name="connection">The connection to use.</param>
        Task InitializeAsync(LocalRepositoryModel repository, IConnection connection);
    }
}
