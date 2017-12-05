using GitHub.UI;

namespace GitHub.ViewModels
{
    public interface IGitHubPaneViewModel : IViewModel
    {
        string ActiveRepoName { get; }
        IView Control { get; }
        string Message { get; }
        MessageType MessageType { get; }

        /// <summary>
        /// Gets a value indicating whether search is available on the current page.
        /// </summary>
        bool IsSearchEnabled { get; }

        /// <summary>
        /// Gets or sets the search query for the current page.
        /// </summary>
        string SearchQuery { get; set; }
    }
}