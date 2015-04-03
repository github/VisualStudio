using GitHub.Caches;
using GitHub.Models;

namespace GitHub.Factories
{
    public interface IAccountFactory
    {
        /// <summary>
        /// Creates an instance of an account using the account retrieved from the API.
        /// </summary>
        /// <param name="account">The account retrieved from the GitHub API.</param>
        /// <returns></returns>
        IAccount CreateAccount(CachedAccount account);
    }
}
