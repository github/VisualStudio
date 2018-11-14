using System.Reactive;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Defines the view model for the "No Origin Remote" view in the GitHub pane.
    /// </summary>
    public interface INoRemoteOriginViewModel : IPanePageViewModel
    {
        /// <summary>
        /// Gets a command that will allow the user to rename remotes.
        /// </summary>
        ReactiveCommand<Unit, Unit> EditRemotes { get; }
    }
}
