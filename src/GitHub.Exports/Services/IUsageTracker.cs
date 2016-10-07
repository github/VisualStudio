namespace GitHub.Services
{
    public interface IUsageTracker
    {
        void IncrementLaunchCount();
        void IncrementCloneCount();
        void IncrementCreateCount();
        void IncrementPublishCount();
        void IncrementOpenInGitHubCount();
        void IncrementLinkToGitHubCount();
        void IncrementCreateGistCount();
        void IncrementUpstreamPullRequestCount();
        void IncrementLoginCount();
    }
}
