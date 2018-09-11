using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog;

namespace GitHub.Services
{
    /// <summary>
    /// Service for displaying the GitHub for Visual Studio dialog.
    /// </summary>
    /// <remarks>
    /// This is a low-level service used by <see cref="IDialogService"/> to carry out the actual
    /// showing of the dialog. You probably want to use <see cref="IDialogService"/> instead if
    /// you want to show the dialog for login/clone etc.
    /// </remarks>
    public interface IShowDialogService
    {
        /// <summary>
        /// Shows a view model in the dialog.
        /// </summary>
        /// <param name="viewModel">The view model to show.</param>
        /// <returns>
        /// The value returned by the <paramref name="viewModel"/>'s 
        /// <see cref="IDialogContentViewModel.Done"/> observable, or null if the dialog was
        /// canceled.
        /// </returns>
        Task<object> Show(IDialogContentViewModel viewModel);

        /// <summary>
        /// Shows a view model that requires a connection with specifiec scopes in the dialog.
        /// </summary>
        /// <param name="viewModel">The view model to show.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="scopes">The required scopes.</param>
        /// <returns>
        /// If the connection does not have the requested scopes, the user will be invited to log
        /// out and back in.
        /// </returns>
        Task<object> Show<TViewModel>(
            TViewModel viewModel,
            IConnection connection,
            IEnumerable<string> scopes)
                where TViewModel : IDialogContentViewModel, IConnectionInitializedViewModel;

        /// <summary>
        /// Shows a view model that requires a connection in the dialog.
        /// </summary>
        /// <param name="viewModel">The view model to show.</param>
        /// <returns>
        /// The value returned by the <paramref name="viewModel"/>'s 
        /// <see cref="IDialogContentViewModel.Done"/> observable, or null if the dialog was
        /// canceled.
        /// </returns>
        /// <remarks>
        /// The first existing connection will be used. If there is no existing connection, the
        /// login dialog will be shown first.
        /// </remarks>
        Task<object> ShowWithFirstConnection<TViewModel>(TViewModel viewModel)
            where TViewModel : IDialogContentViewModel, IConnectionInitializedViewModel;
    }
}
