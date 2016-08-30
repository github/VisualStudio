using GitHub.VisualStudio;
using System.Runtime.InteropServices;

namespace GitHub.Services
{
    [Guid(Guids.UsageTrackerId)]
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
