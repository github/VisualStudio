using GitHub.Exports;
using GitHub.Services;
using NullGuard;
using System;

namespace GitHub.VisualStudio.Menus
{
    public class OpenLink: LinkMenuBase, IDynamicMenuHandler
    {
        public OpenLink(IGitHubServiceProvider serviceProvider)
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

            var link = await GenerateLink(LinkType.Blob);
            if (link == null)
                return;
            var browser = ServiceProvider.TryGetService<IVisualStudioBrowser>();
            browser?.OpenUrl(link.ToUri());

            await UsageTracker.IncrementOpenInGitHubCount();
        }
    }
}
