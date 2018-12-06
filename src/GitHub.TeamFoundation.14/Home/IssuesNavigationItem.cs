using System;
using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Services;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using Microsoft.TeamFoundation.Controls;
using GitHub.UI;
using GitHub.VisualStudio.UI;

namespace GitHub.VisualStudio.TeamExplorer.Home
{
    [TeamExplorerNavigationItem(IssuesNavigationItemId, NavigationItemPriority.Issues)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IssuesNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string IssuesNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA4";

        readonly Lazy<IVisualStudioBrowser> browser;

        [ImportingConstructor]
        public IssuesNavigationItem(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory,
            Lazy<IVisualStudioBrowser> browser,
            ITeamExplorerServiceHolder holder)
            : base(serviceProvider, apiFactory, holder, Octicon.issue_opened)
        {
            this.browser = browser;
            Text = Resources.IssuesNavigationItemText;
            ArgbColor = Colors.LightBlueNavigationItem.ToInt32();
        }

        public override void Execute()
        {
            OpenInBrowser(browser, "issues");
            base.Execute();
        }

        public override async void Invalidate()
        {
            IsVisible = false;

            var visible = await IsAGitHubRepo(ActiveRepoUri);
            if (visible)
            {
                var repo = await SimpleApiClient.GetRepository();
                visible = repo.HasIssues;
            }
            IsVisible = visible;
        }
    }
}
