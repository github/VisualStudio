using System.Windows.Input;

namespace GitHub.UI
{
    public interface ITwoFactorViewModel
    {
        ICommand OkCmd { get; }
        ICommand CancelCmd { get; }
        ICommand ShowHelpCmd { get; }
        ICommand ResendCodeCmd { get; }

        bool IsShowing { get; }
        bool IsSms { get; }
        bool IsAuthenticationCodeSent { get; }
        string Description { get; }
        string AuthenticationCode { get; set; }
    }
}
