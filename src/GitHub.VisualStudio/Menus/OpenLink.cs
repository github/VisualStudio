using GitHub.Extensions;
using GitHub.Services;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using NullGuard;
using GitHub.Api;

namespace GitHub.VisualStudio.Menus
{
    [Export(typeof(IDynamicMenuHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class OpenLink: LinkMenuBase, IDynamicMenuHandler
    {
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        public OpenLink([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            IUsageTracker usageTracker, ISimpleApiClientFactory apiFactory)
            : base(serviceProvider, apiFactory)
        {
            this.usageTracker = usageTracker;
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
            var browser = ServiceProvider.GetExportedValue<IVisualStudioBrowser>();
            browser?.OpenUrl(link.ToUri());

            usageTracker.IncrementOpenInGitHubCount();
        }

        public bool CanShow()
        {
            var githubRepoCheckTask = IsGitHubRepo();
            return githubRepoCheckTask.Wait(250) ? githubRepoCheckTask.Result : false;
        }
    }
}
