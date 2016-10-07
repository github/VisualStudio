using System.Collections.Generic;
using Octokit;

namespace GitHub.Models
{
    /// <summary>
    /// Holds an <see cref="Octokit.User"/> model together with the OAuth scopes that were 
    /// received when the user was read.
    /// </summary>
    public class UserAndScopes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserAndScopes"/> class.
        /// </summary>
        /// <param name="user">The user information.</param>
        /// <param name="scopes">The scopes. May be null.</param>
        public UserAndScopes(User user, IReadOnlyList<string> scopes)
        {
            User = user;
            Scopes = scopes;
        }

        /// <summary>
        /// Gets the user information.
        /// </summary>
        public User User { get; }

        /// <summary>
        /// Gets the OAuth scopes read when the user was read.
        /// </summary>
        /// <remarks>
        /// A value of null means that no X-OAuth-Scopes header was received.
        /// </remarks>
        public IReadOnlyList<string> Scopes { get; }
    }
}
