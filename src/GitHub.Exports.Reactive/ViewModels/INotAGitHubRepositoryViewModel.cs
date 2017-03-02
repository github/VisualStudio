using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Defines the view model for the "Sign in to GitHub" view in the GitHub pane.
    /// </summary>
    public interface INotAGitHubRepositoryViewModel : IDialogViewModel
    {
        /// <summary>
        /// Gets the command executed when the user clicks the "Publish to GitHub" link.
        /// </summary>
        IReactiveCommand<object> Publish { get; }
    }
}