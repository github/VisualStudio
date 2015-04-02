using GitHub.Models;
using Octokit;

namespace GitHub.Factories
{
    public interface IAccountFactory
    {
        /// <summary>
        /// Creates an instance of an account using the account retrieved from the API.
        /// </summary>
        /// <param name="repositoryHost">The host for the account.</param>
        /// <param name="account">The account retrieved from the GitHub API.</param>
        /// <returns></returns>
        IAccount CreateAccount(IRepositoryHost repositoryHost, Account account);
    }
}
