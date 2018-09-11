using System;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog.Clone
{
    /// <summary>
    /// ViewModel for the the Clone Repository dialog
    /// </summary>
    public interface IRepositoryCloneViewModel : IDialogContentViewModel, IConnectionInitializedViewModel
    {
        /// <summary>
        /// Gets the view model for the GitHub.com tab.
        /// </summary>
        IRepositorySelectViewModel GitHubTab { get; }

        /// <summary>
        /// Gets the view model for the enterprise tab.
        /// </summary>
        IRepositorySelectViewModel EnterpriseTab { get; }

        /// <summary>
        /// Gets the view model for the URL tab.
        /// </summary>
        IRepositoryUrlViewModel UrlTab { get; }

        /// <summary>
        /// Gets the path to clone the repository to.
        /// </summary>
        string Path { get; set; }

        /// <summary>
        /// Gets an error message that explains why <see cref="Path"/> is not valid.
        /// </summary>
        string PathError { get; }

        /// <summary>
        /// Gets the index of the selected tab.
        /// </summary>
        /// <remarks>
        /// The tabs are: GitHubPage, EnterprisePage, UrlPage.
        /// </remarks>
        int SelectedTabIndex { get; }

        /// <summary>
        /// Gets the command executed when the user clicks "Browse".
        /// </summary>
        ReactiveCommand<object> Browse { get; }

        /// <summary>
        /// Gets the command executed when the user clicks "Clone".
        /// </summary>
        ReactiveCommand<CloneDialogResult> Clone { get; }
    }
}
