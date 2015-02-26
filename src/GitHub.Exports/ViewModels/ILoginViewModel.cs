using System;
using System.Windows.Input;
using GitHub.Authentication;

namespace GitHub.ViewModels
{
    public interface ILoginViewModel : IViewModel
    {
        string UsernameOrEmail { get; set; }
        string Password { get; set; }
        ICommand LoginCmd { get; }
        ICommand CancelCmd { get; }
        IObservable<object> Cancelling { get; }
        IObservable<AuthenticationResult> AuthenticationResults { get; }
    }
}
