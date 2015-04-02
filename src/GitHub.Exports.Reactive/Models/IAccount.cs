using Octokit;

namespace GitHub.Models
{
    public interface IAccount
    {
        string Email { get; }
        int Id { get; }
        bool IsEnterprise { get; }
        bool IsGitHub { get; }
        bool IsOnFreePlan { get; }
        bool HasMaximumPrivateRepositories { get; }
        bool IsUser { get; }
        /// <summary>
        /// Returns true if the user is a member of the GitHub staff.
        /// </summary>
        IRepositoryHost Host { get; }
        string Login { get; }
        string Name { get; }
        int OwnedPrivateRepos { get; }
        long PrivateReposInPlan { get; }

        void Update(User ghUser);
        void Update(Organization org);
    }
}
