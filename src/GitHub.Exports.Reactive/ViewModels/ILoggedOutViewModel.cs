using System;
using System.Reactive;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Defines the view model for the "Sign in to GitHub" view in the GitHub pane.
    /// </summary>
    public interface ILoggedOutViewModel : IDialogViewModel
    {
        /// <summary>
        /// Gets the command executed when the user clicks the "Sign in" link.
        /// </summary>
        IReactiveCommand<object> SignIn { get; }

        /// <summary>
        /// Gets the command executed when the user clicks the "Create an Account" link.
        /// </summary>
        IReactiveCommand<object> Register { get; }
    }
}