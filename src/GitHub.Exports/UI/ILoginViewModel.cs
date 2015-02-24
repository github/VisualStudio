using GitHub.Authentication;
using System;
using System.Windows.Input;

namespace GitHub.UI
{
    public interface ILoginViewModel
    {
        string UsernameOrEmail { get; set; }
        string Password { get; set; }
        ICommand LoginCmd { get; }
        ICommand CancelCmd { get; }
        IObservable<object> Cancelling { get; }
        IObservable<AuthenticationResult> AuthenticationResults { get; }
    }
}
