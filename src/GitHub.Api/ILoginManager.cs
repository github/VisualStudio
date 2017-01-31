using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GitHub.Primitives;
using GitHub.VisualStudio;
using Octokit;

namespace GitHub.Api
{
    /// <summary>
    /// Provides services for logging into a GitHub server.
    /// </summary>
    [Guid(Guids.LoginManagerId)]
    public interface ILoginManager
    {
        /// <summary>
        /// Attempts to log into a GitHub server.
        /// </summary>
        /// <param name="hostAddress">The address of the server.</param>
        /// <param name="client">An octokit client configured to access the server.</param>
        /// <param name="userName">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>The logged in user.</returns>
        /// <exception cref="AuthorizationException">
        /// The login authorization failed.
        /// </exception>
        Task<User> Login(HostAddress hostAddress, IGitHubClient client, string userName, string password);

        /// <summary>
        /// Attempts to log into a GitHub server using existing credentials.
        /// </summary>
        /// <param name="hostAddress">The address of the server.</param>
        /// <param name="client">An octokit client configured to access the server.</param>
        /// <returns>The logged in user.</returns>
        /// <exception cref="AuthorizationException">
        /// The login authorization failed.
        /// </exception>
        Task<User> LoginFromCache(HostAddress hostAddress, IGitHubClient client);

        /// <summary>
        /// Logs out of GitHub server.
        /// </summary>
        /// <param name="hostAddress">The address of the server.</param>
        Task Logout(HostAddress hostAddress, IGitHubClient client);
    }
}