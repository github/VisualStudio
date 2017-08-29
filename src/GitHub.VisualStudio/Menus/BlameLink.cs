using GitHub.Exports;
using GitHub.Services;
using GitHub.VisualStudio.UI;
using System;
using System.Windows;
using GitHub.Extensions;

namespace GitHub.VisualStudio.Menus
{
    public class BlameLink : LinkMenuBase, IDynamicMenuHandler
    {
        public BlameLink(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
        }

        public Guid Guid => Guids.guidContextMenuSet;
        public int CmdId => PkgCmdIDList.blameCommand;

        public async void Activate(object data = null)
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
