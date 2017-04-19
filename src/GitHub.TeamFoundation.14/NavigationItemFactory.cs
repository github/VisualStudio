using System;
using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Services;
using GitHub.TeamFoundation;
using GitHub.VisualStudio.TeamExplorer.Home;

namespace GitHub.VisualStudio.TeamExplorer
{
    // For some reason this doesn't work when `Shared` (buttons don't appear).
    // [Export, PartCreationPolicy(CreationPolicy.Shared)]
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class NavigationItemFactory
    {
        IGitHubServiceProvider serviceProvider;
        ISimpleApiClientFactory apiFactory;
        Lazy<IVisualStudioBrowser> browser;
        ITeamExplorerServiceHolder holder;
        IMenuProvider menuProvider;

        [ImportingConstructor]
        public NavigationItemFactory(
            IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory,
            Lazy<IVisualStudioBrowser> browser,
            ITeamExplorerServiceHolder holder,
            IMenuProvider menuProvider)
        {
            this.serviceProvider = serviceProvider;
            this.apiFactory = apiFactory;
            this.browser = browser;
            this.holder = holder;
            this.menuProvider = menuProvider;
        }

        [ResolvingTeamExplorerNavigationItem("5245767A-B657-4F8E-BFEE-F04159F1DDA5", NavigationItemPriority.Graphs)]
        //[ExportMetadata("Id", "5245767A-B657-4F8E-BFEE-F04159F1DDA5" /*GraphsNavigationItem.GraphsNavigationItemId*/)]
        //[ExportMetadata("Priority", NavigationItemPriority.Graphs /*NavigationItemPriority.Graphs*/)]
        //[ExportMetadata("TargetPageId", null)]
        public object Graphs
        {
            get
            {
                return TeamFoundationResolver.Resolve(() =>
                    new GraphsNavigationItem(serviceProvider, apiFactory, browser, holder));
            }
        }

        [ResolvingTeamExplorerNavigationItem("5245767A-B657-4F8E-BFEE-F04159F1DDA4", NavigationItemPriority.Issues)]
        //[ExportMetadata("Id", "5245767A-B657-4F8E-BFEE-F04159F1DDA4" /*IssuesNavigationItem.IssuesNavigationItemId*/)]
        //[ExportMetadata("Priority", NavigationItemPriority.Issues)]
        //[ExportMetadata("TargetPageId", null)]
        public object Issues
        {
            get
            {
                return TeamFoundationResolver.Resolve(() =>
                    new IssuesNavigationItem(serviceProvider, apiFactory, browser, holder));
            }
        }

        [ResolvingTeamExplorerNavigationItem("5245767A-B657-4F8E-BFEE-F04159F1DDA3", NavigationItemPriority.PullRequests)]
        //[ExportMetadata("Id", "5245767A-B657-4F8E-BFEE-F04159F1DDA3" /*PullRequestsNavigationItem.PullRequestsNavigationItemId*/)]
        //[ExportMetadata("Priority", NavigationItemPriority.PullRequests)]
        //[ExportMetadata("TargetPageId", null)]
        public object PullRequests
        {
            get
            {
                return TeamFoundationResolver.Resolve(() =>
                    new PullRequestsNavigationItem(serviceProvider, apiFactory, holder, menuProvider));
            }
        }

        [ResolvingTeamExplorerNavigationItem("5245767A-B657-4F8E-BFEE-F04159F1DDA2", NavigationItemPriority.Pulse)]
        //[ExportMetadata("Id", "5245767A-B657-4F8E-BFEE-F04159F1DDA2" /*PulseNavigationItem.PulseNavigationItemId*/)]
        //[ExportMetadata("Priority", NavigationItemPriority.Pulse)]
        //[ExportMetadata("TargetPageId", null)]
        public object Pulse
        {
            get
            {
                return TeamFoundationResolver.Resolve(() =>
                    new PullRequestsNavigationItem(serviceProvider, apiFactory, holder, menuProvider));
            }
        }

        [ResolvingTeamExplorerNavigationItem("5245767A-B657-4F8E-BFEE-F04159F1DDA1", NavigationItemPriority.Wiki)]
        //[ExportMetadata("Id", "5245767A-B657-4F8E-BFEE-F04159F1DDA1" /*WikiNavigationItem.WikiNavigationItemId*/)]
        //[ExportMetadata("Priority", NavigationItemPriority.Wiki)]
        //[ExportMetadata("TargetPageId", null)]
        public object Wiki
        {
            get
            {
                return TeamFoundationResolver.Resolve(() =>
                    new WikiNavigationItem(serviceProvider, apiFactory, browser, holder));
            }
        }
    }
}
