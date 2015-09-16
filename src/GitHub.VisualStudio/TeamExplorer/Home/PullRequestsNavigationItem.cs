using System;
using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Services;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using Microsoft.TeamFoundation.Controls;
using GitHub.UI;

namespace GitHub.VisualStudio.TeamExplorer.Home
{
    [TeamExplorerNavigationItem(PullRequestsNavigationItemId, NavigationItemPriority.PullRequests)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestsNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string PullRequestsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA3";

        readonly Lazy<IVisualStudioBrowser> browser;

        [ImportingConstructor]
        public PullRequestsNavigationItem(ISimpleApiClientFactory apiFactory, Lazy<IVisualStudioBrowser> browser,
                                    ITeamExplorerServiceHolder holder)
            : base(apiFactory, holder, Octicon.git_pull_request)
        {
            this.browser = browser;
            Text = Resources.PullRequestsNavigationItemText;
            ArgbColor = Colors.RedNavigationItem.ToInt32();
        }

        public override void Execute()
        {
            OpenInBrowser(browser, "pulls");
            base.Execute();
        }
    }
}
