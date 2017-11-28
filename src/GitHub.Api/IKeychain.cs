using System;
using System.Threading.Tasks;
using GitHub.Primitives;

namespace GitHub.Api
{
    /// <summary>
    /// Represents a keychain used to store credentials.
    /// </summary>
    public interface IKeychain
    {
        /// <summary>
        /// Loads the credentials for the specified host address.
        /// </summary>
        /// <param name="address">The host address.</param>
        /// <returns>
        /// A task returning a tuple consisting of the retrieved username and password or null
        /// if the credentials were not found.
        /// </returns>
        Task<Tuple<string, string>> Load(HostAddress address);

        /// <summary>
        /// Saves the credentials for the specified host address.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="address">The host address.</param>
        /// <returns>A task tracking the operation.</returns>
        Task Save(string userName, string password, HostAddress address);

        /// <summary>
        /// Deletes the login details for the specified host address.
        /// </summary>
        /// <param name="address"></param>
        /// <returns>A task tracking the operation.</returns>
        Task Delete(HostAddress address);
    }
}
