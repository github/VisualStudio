using System;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// A view model that represents a searchable page in the GitHub pane.
    /// </summary>
    public interface ISearchablePageViewModel : IPanePageViewModel
    {
        /// <summary>
        /// Gets or sets the current search query.
        /// </summary>
        string SearchQuery { get; set; }
    }
}