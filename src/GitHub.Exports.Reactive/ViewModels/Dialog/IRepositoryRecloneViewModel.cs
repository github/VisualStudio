using System;
using GitHub.Models;

namespace GitHub.ViewModels.Dialog
{
    public interface IRepositoryRecloneViewModel : IDialogContentViewModel, IConnectionInitializedViewModel
    {
        /// <summary>
        /// Gets or sets the repository to clone.
        /// </summary>
        RepositoryModel SelectedRepository { get; set; }
    }
}
