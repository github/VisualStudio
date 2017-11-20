using System;

namespace GitHub.ViewModels
{
    /// <summary>
    /// A view model that represents a searchable page in the GitHub pane.
    /// </summary>
    public interface ISearchablePanePageViewModel : IPanePageViewModel
    {
        /// <summary>
        /// Gets or sets the current search query.
        /// </summary>
        string SearchQuery { get; set; }
    }
}