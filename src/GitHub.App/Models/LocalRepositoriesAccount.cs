using Octokit;
using ReactiveUI;

namespace GitHub.Models
{
    public class LocalRepositoriesAccount : ReactiveObject, IAccount
    {
        public LocalRepositoriesAccount(IRepositoryHost host)
        {
            Avatar = "pack://application:,,,/GitHub;component/Images/computer.png";
            Email = string.Empty;
            Id = 0;
            IsOnFreePlan = true;
            IsUser = true;
            Host = host;
            Login = "repositories";
            Name = "repositories";
            OwnedPrivateRepos = 0;
            PrivateReposInPlan = 0;
            IsSiteAdmin = false;
            IsGitHubStaff = false;
        }

        public object Avatar { get; private set; }
        public string Email { get; private set; }
        public int Id { get; private set; }
        public bool IsEnterprise { get { return false; } }
        public bool IsGitHub { get { return false; } }
        public bool IsLocal { get { return true; } }
        public bool IsOnFreePlan { get; private set; }
        public bool HasMaximumPrivateRepositories { get; private set; }
        public bool IsUser { get; private set; }
        public bool IsSiteAdmin { get; private set; }
        public bool IsGitHubStaff { get; private set; }
        public IRepositoryHost Host { get; private set; }
        public string Login { get; private set; }
        public string Name { get; private set; }
        public int OwnedPrivateRepos { get; private set; }
        public long PrivateReposInPlan { get; private set; }

        public void Update(User ghUser)
        {
        }

        public void Update(Organization org)
        {
        }
    }
}
