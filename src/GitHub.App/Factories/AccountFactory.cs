using System.ComponentModel.Composition;
using GitHub.Models;
using Account = GitHub.Models.Account;

namespace GitHub.Factories
{
    [Export(typeof(IAccountFactory))]
    public class AccountFactory : IAccountFactory
    {
        public IAccount CreateAccount(IRepositoryHost repositoryHost, Octokit.Account user)
        {
            return new Account(user, repositoryHost.IsGitHub);
        }
    }
}
