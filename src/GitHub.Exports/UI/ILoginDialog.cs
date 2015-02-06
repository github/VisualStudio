using System;
using System.Windows.Input;

namespace GitHub.Exports
{
    public interface ILoginDialog
    {
        string UsernameOrEmail { get; set; }
        string Password { get; set; }
        ICommand LoginCmd { get; }
        ICommand CancelCmd { get; }
        IObservable<object> CancelEvt { get; }
        IObservable<AuthenticationResult> AuthenticationResults { get; }
    }
}
