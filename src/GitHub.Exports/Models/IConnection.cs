using System;
using System.ComponentModel;
using GitHub.Primitives;
using Octokit;

namespace GitHub.Models
{
    /// <summary>
    /// Represents a configured connection to a GitHub account.
    /// </summary>
    public interface IConnection : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the host address of the GitHub instance.
        /// </summary>
        HostAddress HostAddress { get; }

        /// <summary>
        /// Gets the username of the GitHub account.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Gets the logged in user.
        /// </summary>
        /// <remarks>
        /// This may be null if <see cref="IsLoggedIn"/> is false.
        /// </remarks>
        User User { get; }

        /// <summary>
        /// Gets the login scopes.
        /// </summary>
        ScopesCollection Scopes { get; }

        /// <summary>
        /// Gets a value indicating whether the login of the account succeeded.
        /// </summary>
        bool IsLoggedIn { get; }

        /// <summary>
        /// Gets a value indicating whether a login is currently being attempted on the connection.
        /// </summary>
        bool IsLoggingIn { get; }

        /// <summary>
        /// Gets the exception that occurred when trying to log in, if <see cref="IsLoggedIn"/> is
        /// false.
        /// </summary>
        Exception ConnectionError { get; }
    }
}
