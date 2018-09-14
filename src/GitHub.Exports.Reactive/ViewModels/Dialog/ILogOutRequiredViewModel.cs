using System;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    /// <summary>
    /// Represents the "Logout required" dialog page.
    /// </summary>
    public interface ILogOutRequiredViewModel : IDialogContentViewModel
    {
        /// <summary>
        /// Gets a command that will log out the user.
        /// </summary>
        ReactiveCommand<object> LogOut { get; }
    }
}
