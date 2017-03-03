using System;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.Services
{
    /// <summary>
    /// Services for showing dialogs.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows the clone dialog.
        /// </summary>
        /// <param name="connection">The connection to use.</param>
        /// <returns>
        /// A task that returns an instance of <see cref="CloneDialogResult"/> on success,
        /// or null if the dialog was cancelled.
        /// </returns>
        Task<CloneDialogResult> ShowCloneDialog(IConnection connection);

        /// <summary>
        /// Shows the re-clone dialog.
        /// </summary>
        /// <param name="repository">The repository to clone.</param>
        /// <returns>
        /// A task that returns the base path for the clone on success, or null if the dialog was
        /// cancelled.
        /// </returns>
        /// <remarks>
        /// The re-clone dialog is shown from the VS2017+ start page when the user wants to check
        /// out a repository that was previously checked out on another machine.
        /// </remarks>
        Task<string> ShowReCloneDialog(IRepositoryModel repository);
    }
}
