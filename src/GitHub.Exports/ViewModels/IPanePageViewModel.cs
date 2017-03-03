using System;

namespace GitHub.ViewModels
{
    /// <summary>
    /// A view model that represents a page in the GitHub pane.
    /// </summary>
    public interface IPanePageViewModel : IViewModel
    {
        /// <summary>
        /// Gets the title to display in the pane when the page is shown.
        /// </summary>
        string Title { get; }
    }
}