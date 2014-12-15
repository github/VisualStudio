using GitHub.Models;

namespace GitHub
{
    public interface IAccountFactory
    {
        IAccount CreateAccount(
            IRepositoryHost repositoryHost,
            Octokit.User user);
        
        IAccount CreateAccount(
            IRepositoryHost repositoryHost,
            Octokit.Organization organization);
    }
}
