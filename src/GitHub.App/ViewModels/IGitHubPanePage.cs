using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Implemented by pages in a <see cref="GitHubPaneViewModel"/>.
    /// </summary>
    public interface IGitHubPanePage
    {
        /// <summary>
        /// Gets a command used to refresh the page.
        /// </summary>
        ReactiveCommand<object> Refresh { get; }
    }
}
