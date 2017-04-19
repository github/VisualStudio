using System;
using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Services;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using Microsoft.TeamFoundation.Controls;
using GitHub.UI;
using GitHub.VisualStudio.UI;
using System.Linq;
using GitHub.Extensions;
using GitHub.Exports;
using GitHub.TeamFoundation;

namespace GitHub.VisualStudio.TeamExplorer.Home
{
    //[ResolvingTeamExplorerNavigationItem(PullRequestsNavigationItemId, NavigationItemPriority.PullRequests)]
    //[PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestsNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string PullRequestsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA3";

        readonly IMenuProvider menuProvider;

        [ImportingConstructor]
        public PullRequestsNavigationItem(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory,
            ITeamExplorerServiceHolder holder,
            IMenuProvider menuProvider)
            : base(serviceProvider, apiFactory, holder, Octicon.git_pull_request)
        {
            this.menuProvider = menuProvider;
            Text = Resources.PullRequestsNavigationItemText;
            ArgbColor = Colors.RedNavigationItem.ToInt32();
        }

        public override void Execute()
        {
            var menu = menuProvider.Menus.FirstOrDefault(m => m.IsMenuType(MenuType.OpenPullRequests));
            menu?.Activate(UIControllerFlow.PullRequestList);
            base.Execute();
        }
    }
}
