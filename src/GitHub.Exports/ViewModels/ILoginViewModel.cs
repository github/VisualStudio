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
        string LoginFailedText { get; }
        Uri ForgotPasswordUrl { get; }
        bool IsLoginInProgress { get; }
        bool LoginFailed { get; }
        ICommand LoginCmd { get; }
        ICommand SignUpCommand { get; }
        ICommand ForgotPasswordCommand { get; }
        IObservable<AuthenticationResult> AuthenticationResults { get; }
    }
}
