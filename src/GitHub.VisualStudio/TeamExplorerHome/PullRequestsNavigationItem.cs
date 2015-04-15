using System;
using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Services;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio.TeamExplorerHome
{
    [TeamExplorerNavigationItem(PullRequestsNavigationItemId,
        NavigationItemPriority.PullRequests,
        TargetPageId = TeamExplorerPageIds.Home)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestsNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string PullRequestsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA3";

        readonly Lazy<IVisualStudioBrowser> browser;

        [ImportingConstructor]
        public PullRequestsNavigationItem(ISimpleApiClientFactory apiFactory, Lazy<IVisualStudioBrowser> browser,
                                    ITeamExplorerServiceHolder holder)
            : base(apiFactory, holder)
        {
            this.browser = browser;
            Text = "Pull Requests";
            Image = Resources.git_pull_request;
            ArgbColor = Colors.RedNavigationItem.ToInt32();
        }

        public override void Execute()
        {
            OpenInBrowser(browser, "pulls");
            base.Execute();
        }

        protected override async void UpdateState()
        {
            IsVisible = IsEnabled = await Refresh().ConfigureAwait(true);
        }
    }
}
