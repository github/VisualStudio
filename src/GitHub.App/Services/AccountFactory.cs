using System;
using System.ComponentModel.Composition;
using GitHub.Models;

namespace GitHub
{
    [Export(typeof(IAccountFactory))]
    public class AccountFactory : IAccountFactory
    {
        public IAccount CreateAccount(
            IRepositoryHost repositoryHost,
            Octokit.User user)
        {
            return new Account(repositoryHost, user);
        }

        public IAccount CreateAccount(
            IRepositoryHost repositoryHost,
            Octokit.Organization organization)
        {
            return new Account(repositoryHost, organization);
        }
    }
}
