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
    [TeamExplorerNavigationItem(PullRequestsNavigationItemId,
        NavigationItemPriority.PullRequests,
        TargetPageId = TeamExplorerPageIds.Home)]
    class PullRequestsNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string PullRequestsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA3";

        readonly Lazy<IBrowser> browser;

        [ImportingConstructor]
        public PullRequestsNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory, Lazy<IBrowser> browser)
            : base(serviceProvider, apiFactory)
        {
            this.browser = browser;
            Text = "Pull Requests";
            IsVisible = false;
            IsEnabled = false;
            Image = Resources.git_pull_request;
            ArgbColor = Colors.RedNavigationItem.ToInt32();

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
            var wiki = new Uri(repo.HtmlUrl + "/pulls");
            b.OpenUrl(wiki);
            base.Execute();
        }

        async void UpdateState()
        {
            IsVisible = IsEnabled = await Refresh();
        }
    }
}
