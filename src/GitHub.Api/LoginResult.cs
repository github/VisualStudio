using System;
using System.Collections.Generic;
using GitHub.Models;
using Octokit;

namespace GitHub.Api
{
    /// <summary>
    /// Holds the result of a successful login by <see cref="ILoginManager"/>.
    /// </summary>
    public class LoginResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginResult"/> class.
        /// </summary>
        /// <param name="user">The logged-in user.</param>
        /// <param name="scopes">The login scopes.</param>
        public LoginResult(User user, ScopesCollection scopes)
        {
            User = user;
            Scopes = scopes;
        }

        /// <summary>
        /// Gets the login scopes.
        /// </summary>
        public ScopesCollection Scopes { get; }

        /// <summary>
        /// Gets the logged-in user.
        /// </summary>
        public User User { get; }
    }
}
