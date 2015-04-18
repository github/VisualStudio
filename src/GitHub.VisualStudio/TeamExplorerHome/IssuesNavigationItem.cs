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
    [TeamExplorerNavigationItem(IssuesNavigationItemId,
        NavigationItemPriority.Issues)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IssuesNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string IssuesNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA4";

        readonly Lazy<IVisualStudioBrowser> browser;

        [ImportingConstructor]
        public IssuesNavigationItem(ISimpleApiClientFactory apiFactory, Lazy<IVisualStudioBrowser> browser,
                                    ITeamExplorerServiceHolder holder)
            : base(apiFactory, holder)
        {
            this.browser = browser;
            Text = "Issues";
            Image = Resources.issue_opened;
            ArgbColor = Colors.LightBlueNavigationItem.ToInt32();
        }

        public override void Execute()
        {
            OpenInBrowser(browser, "issues");
            base.Execute();
        }

        protected override async void UpdateState()
        {
            bool visible = await Refresh().ConfigureAwait(true);
            if (visible)
            {
                var repo = await SimpleApiClient.GetRepository().ConfigureAwait(true);
                visible = repo != null && repo.HasIssues;
            }

            IsVisible = IsEnabled = visible;
        }
    }
}
