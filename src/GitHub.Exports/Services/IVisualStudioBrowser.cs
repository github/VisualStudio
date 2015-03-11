using System;

namespace GitHub.Services
{
    /// <summary>
    /// A service used to navigate to URLs such as GitHub issues, pull requests, etc.
    /// </summary>
    /// <remarks>
    /// This interface is (and needs to be) safe to use within TeamExplorer. It should not pull in other dependencies.
    /// </remarks>
    public interface IVisualStudioBrowser
    {
        /// <summary>
        /// Opens the user's default browser to the specified URL.
        /// </summary>
        /// <param name="url">The absolute URI to open</param>
        void OpenUrl(Uri url);
    }
}
