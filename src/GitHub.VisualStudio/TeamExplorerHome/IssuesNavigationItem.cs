using System;
using System.ComponentModel.Composition;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;
using GitHub.Api;
using GitHub.Services;
using GitHub.VisualStudio.Helpers;

namespace GitHub.VisualStudio
{
    [TeamExplorerNavigationItem(IssuesNavigationItemId,
        NavigationItemPriority.Issues,
        TargetPageId = TeamExplorerPageIds.Home)]
    class IssuesNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string IssuesNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA4";

        readonly Lazy<IBrowser> browser;

        [ImportingConstructor]
        public IssuesNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory, Lazy<IBrowser> browser)
            : base(serviceProvider, apiFactory)
        {
            this.browser = browser;
            Text = "Issues";
            IsVisible = false;
            IsEnabled = false;
            Image = Resources.issue_opened;
            ArgbColor = Colors.LightBlueNavigationItem.ToInt32();

            UpdateState();
        }

        protected override void ContextChanged(object sender, ContextChangedEventArgs e)
        {
            UpdateState();
            base.ContextChanged(sender, e);
        }

        public override async void Execute()
        {
            var b = browser.Value;
            var repo = await SimpleApiClient.GetRepository();
            var wiki = new Uri(repo.HtmlUrl + "/issues");
            b.OpenUrl(wiki);
            base.Execute();
        }

        async void UpdateState()
        {
            bool visible = await Refresh();
            if (visible)
            {
                var repo = await SimpleApiClient.GetRepository();
                visible = repo != null && repo.HasIssues;
            }

            IsVisible = IsEnabled = visible;
        }
    }
}
