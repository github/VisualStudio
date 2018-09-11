using System;
using System.ComponentModel.Composition;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog;
using ReactiveUI;

namespace GitHub.App.ViewModels.Dialog
{
    /// <summary>
    /// The "Logout required" dialog page.
    /// </summary>
    [Export(typeof(ILogOutRequiredViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LogOutRequiredViewModel : ViewModelBase, ILogOutRequiredViewModel
    {
        /// <inheritdoc/>
        public ReactiveCommand<object> LogOut { get; } = ReactiveCommand.Create();

        /// <inheritdoc/>
        public string Title => Resources.LogoutRequired;

        /// <inheritdoc/>
        public IObservable<object> Done => LogOut;
    }
}
