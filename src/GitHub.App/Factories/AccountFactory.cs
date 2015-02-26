using System.ComponentModel.Composition;
using GitHub.Models;
using Octokit;
using Account = GitHub.Models.Account;

namespace GitHub.Factories
{
    [Export(typeof(IAccountFactory))]
    public class AccountFactory : IAccountFactory
    {
        public IAccount CreateAccount(
            IRepositoryHost repositoryHost,
            User user)
        {
            return new Account(repositoryHost, user);
        }

        public IAccount CreateAccount(
            IRepositoryHost repositoryHost,
            Organization organization)
        {
            return new Account(repositoryHost, organization);
        }
    }
}
