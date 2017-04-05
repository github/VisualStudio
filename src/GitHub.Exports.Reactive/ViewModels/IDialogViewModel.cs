using System;
using System.Reactive;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// A <see cref="IDialogViewModel"/> which exposes its Cancel command as a reactive command.
    /// </summary>
    public interface IDialogViewModel : IViewModel, IHasDone, IHasCancel
    {
        /// <summary>
        /// Gets a title to display in the dialog titlebar.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets a value indicating whether the view model is busy.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Gets a value indicating whether the view model represents the page currently being shown.
        /// </summary>
        bool IsShowing { get; }

        /// <summary>
        /// Gets a command that will dismiss the page.
        /// </summary>
        new ReactiveCommand<object> Cancel { get; }
    }
}