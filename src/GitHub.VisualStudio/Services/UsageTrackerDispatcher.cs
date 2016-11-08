using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;

namespace GitHub.Services
{
    [Export(typeof(IUsageTracker))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class UsageTrackerDispatcher : IUsageTracker
    {
        readonly IUsageTracker inner;

        [ImportingConstructor]
        public UsageTrackerDispatcher([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            inner = serviceProvider.GetService(typeof(IUsageTracker)) as IUsageTracker;
        }

        public void IncrementCloneCount() => inner.IncrementCloneCount();
        public void IncrementCreateCount() => inner.IncrementCreateCount();
        public void IncrementCreateGistCount() => inner.IncrementCreateGistCount();
        public void IncrementLaunchCount() => inner.IncrementLaunchCount();
        public void IncrementLinkToGitHubCount() => inner.IncrementLinkToGitHubCount();
        public void IncrementLoginCount() => inner.IncrementLoginCount();
        public void IncrementOpenInGitHubCount() => inner.IncrementOpenInGitHubCount();
        public void IncrementPublishCount() => inner.IncrementPublishCount();
        public void IncrementUpstreamPullRequestCount() => inner.IncrementUpstreamPullRequestCount();
    }
}
