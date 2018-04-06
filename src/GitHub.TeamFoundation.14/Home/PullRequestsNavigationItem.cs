using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Commands;
using GitHub.Extensions;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using GitHub.VisualStudio.UI;
using Microsoft.TeamFoundation.Controls;

namespace GitHub.VisualStudio.TeamExplorer.Home
{
    [TeamExplorerNavigationItem(PullRequestsNavigationItemId, NavigationItemPriority.PullRequests)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestsNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string PullRequestsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA3";

        readonly IOpenPullRequestsCommand openPullRequests;

        [ImportingConstructor]
        public PullRequestsNavigationItem(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory,
            ITeamExplorerServiceHolder holder,
            IOpenPullRequestsCommand openPullRequests)
            : base(serviceProvider, apiFactory, holder, Octicon.git_pull_request)
        {
            this.openPullRequests = openPullRequests;
            Text = Resources.PullRequestsNavigationItemText;
            ArgbColor = Colors.RedNavigationItem.ToInt32();
        }

        public override void Execute() => openPullRequests.Execute();
    }
}
