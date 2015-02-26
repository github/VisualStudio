using GitHub.Models;
using Octokit;

namespace GitHub.Factories
{
    public interface IAccountFactory
    {
        IAccount CreateAccount(
            IRepositoryHost repositoryHost,
            User user);
        
        IAccount CreateAccount(
            IRepositoryHost repositoryHost,
            Organization organization);
    }
}
