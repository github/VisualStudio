using System;

namespace GitHub.ViewModels
{
    /// <summary>
    /// A view model that represents a page in the GitHub pane.
    /// </summary>
    public interface IPanePageViewModel : IViewModel
    {
        /// <summary>
        /// Gets the title for the page.
        /// </summary>
        string Title { get; }
    }
}