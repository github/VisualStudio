using System;
using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Services;
using GitHub.TeamFoundation;
using GitHub.VisualStudio.TeamExplorer.Home;

namespace GitHub.VisualStudio.TeamExplorer
{
    // This doesn't work if `CreationPolicy.Shared`.
    [Export, PartCreationPolicy(CreationPolicy.Any)]
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

        [ResolvingTeamExplorerNavigationItem("5245767A-B657-4F8E-BFEE-F04159F1DDA5" /*GraphsNavigationItem.GraphsNavigationItemId*/,
            NavigationItemPriority.Graphs)]
        public object Graphs
        {
            get
            {
                return TeamFoundationResolver.Resolve(() =>
                    new GraphsNavigationItem(serviceProvider, apiFactory, browser, holder));
            }
        }

        [ResolvingTeamExplorerNavigationItem("5245767A-B657-4F8E-BFEE-F04159F1DDA4" /*IssuesNavigationItem.IssuesNavigationItemId*/,
            NavigationItemPriority.Issues)]
        public object Issues
        {
            get
            {
                return TeamFoundationResolver.Resolve(() =>
                    new IssuesNavigationItem(serviceProvider, apiFactory, browser, holder));
            }
        }

        [ResolvingTeamExplorerNavigationItem("5245767A-B657-4F8E-BFEE-F04159F1DDA3" /*PullRequestsNavigationItem.PullRequestsNavigationItemId*/,
            NavigationItemPriority.PullRequests)]
        public object PullRequests
        {
            get
            {
                return TeamFoundationResolver.Resolve(() =>
                    new PullRequestsNavigationItem(serviceProvider, apiFactory, holder, menuProvider));
            }
        }

        [ResolvingTeamExplorerNavigationItem("5245767A-B657-4F8E-BFEE-F04159F1DDA2" /*PulseNavigationItem.PulseNavigationItemId*/,
            NavigationItemPriority.Pulse)]
        public object Pulse
        {
            get
            {
                return TeamFoundationResolver.Resolve(() =>
                    new PullRequestsNavigationItem(serviceProvider, apiFactory, holder, menuProvider));
            }
        }

        [ResolvingTeamExplorerNavigationItem("5245767A-B657-4F8E-BFEE-F04159F1DDA1" /*WikiNavigationItem.WikiNavigationItemId*/,
            NavigationItemPriority.Wiki)]
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
