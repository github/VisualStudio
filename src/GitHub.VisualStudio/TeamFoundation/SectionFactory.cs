using System;
using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Models;
using GitHub.Services;
using GitHub.Settings;
using GitHub.TeamFoundation;
using GitHub.VisualStudio.TeamExplorer.Home;
using GitHub.VisualStudio.TeamExplorer.Connect;
using GitHub.VisualStudio.TeamExplorer.Sync;

namespace GitHub.VisualStudio.TeamExplorer
{
    // Doesn't work if `CreationPolicy.Shared`.
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
        Lazy<IVisualStudioBrowser> visualStudioBrowser;
        Lazy<IRepositoryHosts> repositoryHosts;

        [ImportingConstructor]
        public SectionFactory(
            TeamFoundationResolver teamFoundationResolver,
            IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory,
            ITeamExplorerServiceHolder holder,
            IConnectionManager manager,
            IPackageSettings settings,
            IVSServices vsServices,
            IRepositoryCloneService cloneService,
            IDialogService dialogService,
            Lazy<IVisualStudioBrowser> visualStudioBrowser,
            Lazy<IRepositoryHosts> repositoryHosts)
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
            this.repositoryHosts = repositoryHosts;
        }

        [ResolvingTeamExplorerSection(
            "72008232-2104-4FA0-A189-61B0C6F91198" /*GitHubHomeSection.GitHubHomeSectionId*/,
            "312E8A59-2712-48A1-863E-0EF4E67961FC" /*TeamExplorerPageIds.Home*/, 10)]
        public object Home => new GitHubHomeSection(serviceProvider, apiFactory, holder, visualStudioBrowser.Value);

        [ResolvingTeamExplorerSection(
            "519B47D3-F2A9-4E19-8491-8C9FA25ABE90" /*GitHubConnectSection0.GitHubConnectSection0Id*/,
            "3185ED96-1CBD-4381-A439-636973542E50" /*TeamExplorerPageIds.Connect*/, 10)]
        public object Connect0 => new GitHubConnectSection0(serviceProvider, apiFactory, holder, manager, settings, vsServices, cloneService, dialogService);

        [ResolvingTeamExplorerSection(
            "519B47D3-F2A9-4E19-8491-8C9FA25ABE91" /*GitHubConnectSection1.GitHubConnectSection1Id*/,
            "3185ED96-1CBD-4381-A439-636973542E50" /*TeamExplorerPageIds.Connect*/, 10)]
        public object Connect1 => new GitHubConnectSection1(serviceProvider, apiFactory, holder, manager, settings, vsServices, cloneService, dialogService);

        [ResolvingTeamExplorerSection(
            "92655B52-360D-4BF5-95C5-D9E9E596AC76" /*GitHubPublishSectionId*/,
            "D0E4EA4E-24F0-46D6-9D07-0BC09CDEAE7D" /*TeamExplorerPageIds.GitCommits*/, 10)]
        public object Publish => new GitHubPublishSection(serviceProvider, apiFactory, holder, manager, visualStudioBrowser, repositoryHosts.Value);
    }
}
