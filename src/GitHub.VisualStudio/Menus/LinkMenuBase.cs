using GitHub.Exports;
using GitHub.Primitives;
using GitHub.Services;
using System;
using System.Threading.Tasks;

namespace GitHub.VisualStudio.Menus
{
    public class LinkMenuBase: MenuBase
    {
        readonly Lazy<IUsageTracker> usageTracker;

        protected IUsageTracker UsageTracker => usageTracker.Value;

        public LinkMenuBase(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            usageTracker = new Lazy<IUsageTracker>(() => ServiceProvider.TryGetService<IUsageTracker>());
        }

        protected Task<UriString> GenerateLink(LinkType linkType)
        {
            var repo = ActiveRepo;
            var activeDocument = ServiceProvider.TryGetService<IActiveDocumentSnapshot>();
            if (activeDocument == null)
                return null;
            return repo.GenerateUrl(linkType, activeDocument.Name, activeDocument.StartLine, activeDocument.EndLine);
        }

        public bool CanShow()
        {
            var githubRepoCheckTask = IsGitHubRepo();
            //Set max of 250ms wait time to prevent UI blocking
            return githubRepoCheckTask.Wait(250) ? githubRepoCheckTask.Result : false;
        }
    }
}
