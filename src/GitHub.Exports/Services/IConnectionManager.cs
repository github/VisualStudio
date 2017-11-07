using System;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Services
{
    /// <summary>
    /// Manages the configured <see cref="IConnection"/>s to GitHub instances.
    /// </summary>
    public interface IConnectionManager
    {
        /// <summary>
        /// Gets a collection containing the current connections.
        /// </summary>
        /// <remarks>
        /// This collection is lazily initialized: the first time the <see cref="Connections"/>
        /// property is accessed, an async load of the configured connections is started. If you
        /// want to ensure that all connections have been loaded, call
        /// <see cref="GetLoadedConnections"/>.
        /// </remarks>
        IReadOnlyObservableCollection<IConnection> Connections { get; }

        /// <summary>
        /// Gets a callback that is called after a new <see cref="IConnection"/> is created but
        /// before it is added to <see cref="Connections"/>.
        /// </summary>
        /// <remarks>
        /// This is a hack and should be removed as soon as possible! It's needed because creating
        /// a RepositoryHost is async meaning that for a short time a connection can exist without
        /// a repository host, but other older parts of the code get confused by this. This callback
        /// allows RepositoryHosts to hook into the creation of the connection to make sure a host
        /// is available by the time the connection is made available.
        /// </remarks>
        Func<IConnection, Task> ConnectionCreated { get; set; }

        /// <summary>
        /// Gets a connection with the specified host address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>
        /// A task returning the requested connection, or null if the connection was not found.
        /// </returns>
        Task<IConnection> GetConnection(HostAddress address);

        /// <summary>
        /// Gets the <see cref="Connections"/> collection, after ensuring that it is fully loaded.
        /// </summary>
        /// <returns>
        /// A task returning the fully loaded connections collection.
        /// </returns>
        Task<IReadOnlyObservableCollection<IConnection>> GetLoadedConnections();

        /// <summary>
        /// Attempts to login to a GitHub instance.
        /// </summary>
        /// <param name="address">The instance address.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>
        /// A connection if the login succeded. If the login fails, throws an exception. An
        /// exception is also thrown if an existing connection with the same host address already
        /// exists.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// A connection to the host already exists.
        /// </exception>
        Task<IConnection> LogIn(HostAddress address, string username, string password);

        /// <summary>
        /// Logs out of a GitHub instance.
        /// </summary>
        /// <param name="address"></param>
        /// <returns>A task tracking the operation.</returns>
        Task LogOut(HostAddress address);
    }
}