using System;
using System.Threading.Tasks;
using GitHub.Primitives;

namespace GitHub.Api
{
    /// <summary>
    /// Stores login details.
    /// </summary>
    public interface ILoginCache
    {
        /// <summary>
        /// Gets the login details for the specified host address.
        /// </summary>
        /// <param name="hostAddress">The host address.</param>
        /// <returns>
        /// A task returning a tuple containing the retrieved username and password.
        /// </returns>
        Task<Tuple<string, string>> GetLogin(HostAddress hostAddress);

        /// <summary>
        /// Saves the login details for the specified host address.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="hostAddress">The host address.</param>
        /// <returns>A task tracking the operation.</returns>
        Task SaveLogin(string userName, string password, HostAddress hostAddress);

        /// <summary>
        /// Removes the login details for the specified host address.
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <returns>A task tracking the operation.</returns>
        Task EraseLogin(HostAddress hostAddress);
    }
}
