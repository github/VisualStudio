using System;
using System.Windows.Input;
using GitHub.Authentication;

namespace GitHub.ViewModels
{
    public interface ILoginViewModel : IViewModel
    {
        string UsernameOrEmail { get; set; }
        string Password { get; set; }
        string LoginButtonText { get; }
        bool IsLoginInProgress { get; }
        ICommand LoginCmd { get; }
        IObservable<AuthenticationResult> AuthenticationResults { get; }
    }
}
