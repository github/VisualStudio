using GitHub.Exports;
using GitHub.Services;
using GitHub.VisualStudio.UI;
using NullGuard;
using System;
using System.Windows;

namespace GitHub.VisualStudio.Menus
{
    public class BlameLink : LinkMenuBase, IDynamicMenuHandler
    {
        public BlameLink(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public Guid Guid => GuidList.guidContextMenuSet;
        public int CmdId => PkgCmdIDList.blameCommand;

        public async void Activate([AllowNull]object data = null)
        {
            var isgithub = await IsGitHubRepo();
            if (!isgithub)
                return;

            var link = await GenerateLink(LinkType.Blame);
            if (link == null)
                return;
            var browser = ServiceProvider.TryGetService<IVisualStudioBrowser>();
            browser?.OpenUrl(link.ToUri());

            await UsageTracker.IncrementOpenInGitHubCount();
        }
    }
}
