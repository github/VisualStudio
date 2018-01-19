using System.Reactive;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    /// <summary>
    /// Represents a view model responsible for authenticating a user
    /// against GitHub.com.
    /// </summary>
    public interface ILoginToGitHubViewModel : ILoginToHostViewModel
    {
        /// <summary>
        /// Gets a command which, when invoked, directs the user to
        /// a GitHub.com lost password flow.
        /// </summary>
        IReactiveCommand<Unit> NavigatePricing { get; }
    }
}