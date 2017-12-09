using System;

namespace GitHub.ViewModels.Dialog
{
    /// <summary>
    /// Represents a view that can be shown in the GitHub dialog.
    /// </summary>
    public interface IDialogContentViewModel : IViewModel
    {
        /// <summary>
        /// Gets a title to display in the dialog title bar.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets an observable that is signalled with a return value when the dialog has completed.
        /// </summary>
        IObservable<object> Done { get; }
    }
}
