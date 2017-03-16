using GitHub.UI;
using ReactiveUI;
using System.Diagnostics.CodeAnalysis;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Represents a view model for the "Log out Required" dialog..
    /// </summary>
    public interface ILogoutRequiredViewModel : IDialogViewModel
    {
        IReactiveCommand<ProgressState> Logout { get; }
        string LogoutRequiredMessage { get; set; }
        Octicon Icon { get; set; }
    }
}