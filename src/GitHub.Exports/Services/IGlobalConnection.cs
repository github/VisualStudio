using System;
using GitHub.Models;

namespace GitHub.Services
{
    /// <summary>
    /// Returns the current globally registered <see cref="IConnection"/>.
    /// </summary>
    /// <remarks>
    /// Many view models require a connection with which to work. The UIController registers the
    /// connection for the current flow with <see cref="IGitHubServiceProvider"/> in its Start()
    /// method for this purpose. View models wishing to retreive this value should import this
    /// interface and call <see cref="Get"/>.
    /// </remarks>
    public interface IGlobalConnection
    {
        /// <summary>
        /// Gets the globally registered <see cref="IConnection"/>.
        /// </summary>
        IConnection Get();
    }
}
