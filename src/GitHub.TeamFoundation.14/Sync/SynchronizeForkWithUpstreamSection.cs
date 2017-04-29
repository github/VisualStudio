using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Services;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;

namespace GitHub.VisualStudio.TeamExplorer.Sync
{
    [TeamExplorerSection(SynchronizeForkWithUpstreamSection.SectionId, TeamExplorerPageIds.GitCommits, 1)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SynchronizeForkWithUpstreamSection : TeamExplorerSectionBase, ISynchronizeForkWithUpstreamSection
    {
        public const string SectionId = "BD046265-3E0F-4287-8390-627135905DD8";
        readonly Lazy<ISynchronizeForkService> service;

        [ImportingConstructor]
        public SynchronizeForkWithUpstreamSection(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder,
            IConnectionManager cm, Lazy<ISynchronizeForkService> service)
            : base(serviceProvider, apiFactory, holder, cm)
        {
            this.service = service;
            IsVisible = true;
            Title = "Upstream sync";
            View = new SynchronizeForkWithUpstream { DataContext = this };
        }

        public async Task Synchronize()
        {
            IsBusy = true;
            await service.Value.Sync(ActiveRepo);
            IsBusy = false;
        }

        protected SynchronizeForkWithUpstream View
        {
            get { return SectionContent as SynchronizeForkWithUpstream; }
            set { SectionContent = value; }
        }
    }
}