namespace GitHub.Models
{
    public interface IAccount
    {
        bool IsEnterprise { get; }
        bool IsOnFreePlan { get; }
        bool HasMaximumPrivateRepositories { get; }
        bool IsUser { get; }
        string Login { get; }
        int OwnedPrivateRepos { get; }
        long PrivateReposInPlan { get; }
    }
}
