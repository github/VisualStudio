using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// A <see cref="IDialogViewModel"/> which exposes its Cancel command as a reactive command.
    /// </summary>
    public interface IReactiveDialogViewModel : IDialogViewModel
    {
        /// <summary>
        /// Gets a command representing a cancel command that will close the dialog.
        /// </summary>
        IReactiveCommand<object> CancelCommand { get; }
    }
}