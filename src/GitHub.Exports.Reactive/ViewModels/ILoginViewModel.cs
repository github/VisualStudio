using System;
using GitHub.Authentication;

namespace GitHub.ViewModels
{
    public interface ILoginViewModel : IDialogViewModel
    {
        /// <summary>
        /// Gets an observable sequence which produces an authentication 
        /// result every time a log in attempt through this control success
        /// or fails.
        /// </summary>
        IObservable<AuthenticationResult> AuthenticationResults { get; }
    }
}
