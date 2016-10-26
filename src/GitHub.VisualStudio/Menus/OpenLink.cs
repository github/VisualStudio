using GitHub.Services;
using NullGuard;
using System;

namespace GitHub.VisualStudio.Menus
{
    public class OpenLink: LinkMenuBase, IDynamicMenuHandler
    {
        public OpenLink(IUIProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public Guid Guid => GuidList.guidContextMenuSet;
        public int CmdId => PkgCmdIDList.openLinkCommand;

        public async void Activate([AllowNull]object data = null)
        {
            var isgithub = await IsGitHubRepo();
            if (!isgithub)
                return;

            var link = GenerateLink();
            if (link == null)
                return;
            var browser = ServiceProvider.TryGetService<IVisualStudioBrowser>();
            browser?.OpenUrl(link.ToUri());

            UsageTracker.IncrementOpenInGitHubCount();
        }

        public bool CanShow()
        {
            var githubRepoCheckTask = IsGitHubRepo();
            return githubRepoCheckTask.Wait(250) ? githubRepoCheckTask.Result : false;
        }
    }
}
