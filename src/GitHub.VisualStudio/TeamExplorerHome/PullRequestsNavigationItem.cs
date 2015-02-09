using System;
using System.ComponentModel.Composition;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio
{
    [TeamExplorerNavigationItem(PullRequestsNavigationItemId,
        NavigationItemPriority.PullRequests,
        TargetPageId = TeamExplorerPageIds.Home)]
    class PullRequestsNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string PullRequestsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA3";

        [ImportingConstructor]
        public PullRequestsNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
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

        async void UpdateState()
        {
            var solution = ServiceProvider.GetSolution();
            IsVisible = await solution.IsHostedOnGitHub();
        }
    }
}
