using System;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Represents a view model with a URL that can be opened in the system web browser.
    /// </summary>
    public interface IOpenInBrowser
    {
        /// <summary>
        /// Gets the URL.
        /// </summary>
        Uri WebUrl { get; }
    }
}
