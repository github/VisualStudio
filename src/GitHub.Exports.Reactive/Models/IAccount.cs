using GitHub.Models;
using Octokit;
using ReactiveUI;

namespace GitHub
{
    public interface IAccount
    {
        string Email { get; }
        int Id { get; }
        bool IsEnterprise { get; }
        bool IsGitHub { get; }
        bool IsLocal { get; }
        bool IsOnFreePlan { get; }
        bool HasMaximumPrivateRepositories { get; }
        bool IsUser { get; }
        /// <summary>
        /// True if the user is an admin on the host (GitHub or Enterprise).
        /// </summary>
        /// <remarks>
        /// Do not confuse this with "IsStaff". This is true if the user is an admin 
        /// on the site. IsStaff is true if that site is github.com.
        /// </remarks>
        bool IsSiteAdmin { get; }
        /// <summary>
        /// Returns true if the user is a member of the GitHub staff.
        /// </summary>
        bool IsGitHubStaff { get; }
        IRepositoryHost Host { get; }
        string Login { get; }
        string Name { get; }
        int OwnedPrivateRepos { get; }
        long PrivateReposInPlan { get; }

        void Update(User ghUser);
        void Update(Organization org);
    }
}
