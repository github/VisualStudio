using System;
using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Services
{
    /// <summary>
    /// Represents a configured connection to a GitHub account.
    /// </summary>
    public class Connection : IConnection
    {
        public Connection(
            HostAddress hostAddress,
            string userName,
            Octokit.User user,
            Exception connectionError)
        {
            HostAddress = hostAddress;
            Username = userName;
            User = user;
            ConnectionError = connectionError;
        }

        /// <inheritdoc/>
        public HostAddress HostAddress { get; }

        /// <inheritdoc/>
        public string Username { get; }

        /// <inheritdoc/>
        public Octokit.User User { get; }

        /// <inheritdoc/>
        public bool IsLoggedIn => ConnectionError == null;

        /// <inheritdoc/>
        public Exception ConnectionError { get; }
    }
}
