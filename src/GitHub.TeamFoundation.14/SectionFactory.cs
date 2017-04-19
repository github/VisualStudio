using System;
using System.Diagnostics;
using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Models;
using GitHub.Services;
using GitHub.Settings;
using GitHub.TeamFoundation;
using GitHub.VisualStudio.TeamExplorer.Home;
using GitHub.VisualStudio.TeamExplorer.Connect;

namespace GitHub.VisualStudio.TeamExplorer
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class SectionFactory
    {
        IGitHubServiceProvider serviceProvider;
        ISimpleApiClientFactory apiFactory;
        ITeamExplorerServiceHolder holder;
        IConnectionManager manager;
        IPackageSettings settings;
        IVSServices vsServices;
        IRepositoryCloneService cloneService;
        IDialogService dialogService;
        IVisualStudioBrowser visualStudioBrowser;

        [ImportingConstructor]
        public SectionFactory(
            IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory,
            ITeamExplorerServiceHolder holder,
            IConnectionManager manager,
            IPackageSettings settings,
            IVSServices vsServices,
            IRepositoryCloneService cloneService,
            IDialogService dialogService,
            IVisualStudioBrowser visualStudioBrowser)
        {
            this.serviceProvider = serviceProvider;
            this.apiFactory = apiFactory;
            this.holder = holder;
            this.manager = manager;
            this.settings = settings;
            this.vsServices = vsServices;
            this.cloneService = cloneService;
            this.dialogService = dialogService;
            this.visualStudioBrowser = visualStudioBrowser;
        }

        [ResolvingTeamExplorerSection/*("72008232-2104-4FA0-A189-61B0C6F91198", "312E8A59-2712-48A1-863E-0EF4E67961FC", 10)*/]
        [ExportMetadata("Id", "72008232-2104-4FA0-A189-61B0C6F91198" /*GitHubHomeSection.GitHubHomeSectionId*/)]
        [ExportMetadata("ParentPageId", "312E8A59-2712-48A1-863E-0EF4E67961FC" /*TeamExplorerPageIds.Home*/)]
        [ExportMetadata("Priority", 10)]
        public object Home
        {
            get
            {
                return TeamFoundationResolver.Resolve(() =>
                    new GitHubHomeSection(serviceProvider, apiFactory, holder, visualStudioBrowser));
            }
        }

        [ResolvingTeamExplorerSection/*("519B47D3-F2A9-4E19-8491-8C9FA25ABE90", "519B47D3-F2A9-4E19-8491-8C9FA25ABE90", 10)*/]
        [ExportMetadata("Id", "519B47D3-F2A9-4E19-8491-8C9FA25ABE90" /*GitHubConnectSection0.GitHubConnectSection0Id*/)]
        [ExportMetadata("ParentPageId", "3185ED96-1CBD-4381-A439-636973542E50" /*TeamExplorerPageIds.Connect*/)]
        [ExportMetadata("Priority", 10)]
        public object Connect0
        {
            get
            {
                using (new TeamFoundationResolver())
                {
                    return TeamFoundationResolver.Resolve(() =>
                        new GitHubConnectSection0(serviceProvider, apiFactory, holder, manager, settings, vsServices, cloneService, dialogService));
                }
            }
        }

        [ResolvingTeamExplorerSection/*("519B47D3-F2A9-4E19-8491-8C9FA25ABE91", "3185ED96-1CBD-4381-A439-636973542E50", 10)*/]
        [ExportMetadata("Id", "519B47D3-F2A9-4E19-8491-8C9FA25ABE91" /*GitHubConnectSection1.GitHubConnectSection1Id*/)]
        [ExportMetadata("ParentPageId", "3185ED96-1CBD-4381-A439-636973542E50" /*TeamExplorerPageIds.Connect*/)]
        [ExportMetadata("Priority", 10)]
        public object Connect1
        {
            get
            {
                return TeamFoundationResolver.Resolve(() =>
                    new GitHubConnectSection1(serviceProvider, apiFactory, holder, manager, settings, vsServices, cloneService, dialogService));
            }
        }
    }
}
