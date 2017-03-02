using System.Windows.Input;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Represents a view model that can be dismissed, such as a dialog.
    /// </summary>
    public interface IDialogViewModel : IViewModel
    {
        /// <summary>
        /// Gets a title to display in the dialog titlebar.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets a cancel command that will dismiss the page.
        /// </summary>
        ICommand Cancel { get; }

        /// <summary>
        /// Gets a value indicating whether the view model represents the page currently being shown.
        /// </summary>
        bool IsShowing { get; }
    }
}