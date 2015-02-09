using System;
using System.ComponentModel.Composition;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio
{
    [TeamExplorerNavigationItem(GraphsNavigationItemId,
        NavigationItemPriority.Graphs,
        TargetPageId = TeamExplorerPageIds.Home)]
    class GraphsNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string GraphsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA5";

        [ImportingConstructor]
        public GraphsNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Text = "Graphs";
            IsVisible = false;
            IsEnabled = true;
            Image = Resources.graph;

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
