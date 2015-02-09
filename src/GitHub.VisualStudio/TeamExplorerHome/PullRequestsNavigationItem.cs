using System;
using System.ComponentModel.Composition;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;
using GitHub.Api;

namespace GitHub.VisualStudio
{
    [TeamExplorerNavigationItem(PullRequestsNavigationItemId,
        NavigationItemPriority.PullRequests,
        TargetPageId = TeamExplorerPageIds.Home)]
    class PullRequestsNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string PullRequestsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA3";

        [ImportingConstructor]
        public PullRequestsNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory)
            : base(serviceProvider, apiFactory)
        {
            Text = "Pull Requests";
            IsVisible = false;
            IsEnabled = true;
            Image = Resources.git_pull_request;

            UpdateState();
        }

        protected override void ContextChanged(object sender, ContextChangedEventArgs e)
        {
            UpdateState();
            base.ContextChanged(sender, e);
        }

        public override void Execute()
        {
            base.Execute();
        }

        protected override void UpdateState()
        {
            base.UpdateState();

            if (IsVisible)
            {

            }
        }

    }
}
