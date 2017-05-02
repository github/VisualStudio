using System;
using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Models;
using GitHub.Services;
using GitHub.Settings;
using GitHub.VisualStudio.TeamExplorer.Home;
using GitHub.VisualStudio.TeamExplorer.Connect;
using GitHub.VisualStudio.TeamExplorer.Sync;
using Microsoft.TeamFoundation.Controls;

namespace GitHub.VisualStudio.TeamFoundation
{
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

        [ResolvingTeamExplorerSection(GitHubHomeSection.GitHubHomeSectionId, TeamExplorerPageIds.Home, 10)]
        public object Home => new GitHubHomeSection(serviceProvider, apiFactory, holder, visualStudioBrowser.Value);

        [ResolvingTeamExplorerSection(GitHubConnectSection0.GitHubConnectSection0Id, TeamExplorerPageIds.Connect, 10)]
        public object Connect0 => new GitHubConnectSection0(serviceProvider, apiFactory, holder, manager, settings, vsServices, cloneService, dialogService);

        [ResolvingTeamExplorerSection(GitHubConnectSection1.GitHubConnectSection1Id, TeamExplorerPageIds.Connect, 10)]
        public object Connect1 => new GitHubConnectSection1(serviceProvider, apiFactory, holder, manager, settings, vsServices, cloneService, dialogService);

        [ResolvingTeamExplorerSection(GitHubPublishSection.GitHubPublishSectionId, TeamExplorerPageIds.GitCommits, 10)]
        public object Publish => new GitHubPublishSection(serviceProvider, apiFactory, holder, manager, visualStudioBrowser, repositoryHosts.Value);
    }
}
