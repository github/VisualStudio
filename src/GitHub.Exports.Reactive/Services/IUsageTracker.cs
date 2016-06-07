namespace GitHub.Services
{
    public interface IUsageTracker
    {
        void IncrementCloneCount();
        void IncrementCommitCount();
        void IncrementLaunchCount();
        void IncrementUpstreamPullRequestCount();
    }
}
