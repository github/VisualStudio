using System;
using System.Reactive;
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
        ReactiveCommand<Unit, Unit> LogOut { get; }
    }
}
