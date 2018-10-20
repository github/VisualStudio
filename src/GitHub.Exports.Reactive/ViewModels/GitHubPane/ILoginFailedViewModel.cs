using System.Reactive;
using ReactiveUI;
using ReactiveUI.Legacy;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Defines the view model for the "Login Failed" view in the GitHub pane.
    /// </summary>
    public interface ILoginFailedViewModel : IPanePageViewModel
    {
        /// <summary>
        /// Gets a description of the login failure.
        /// </summary>
        UserError LoginError { get; }

        /// <summary>
        /// Gets a command which opens the Team Explorer Connect page.
        /// </summary>
        ReactiveCommand<Unit, Unit> OpenTeamExplorer { get; }

        /// <summary>
        /// Initializes the view model with an error.
        /// </summary>
        /// <param name="error">The error.</param>
        void Initialize(UserError error);
    }
}