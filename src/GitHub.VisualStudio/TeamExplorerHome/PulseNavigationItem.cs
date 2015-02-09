using System;
using System.ComponentModel.Composition;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio
{
    [TeamExplorerNavigationItem(PulseNavigationItemId,
        NavigationItemPriority.Pulse,
        TargetPageId = TeamExplorerPageIds.Home)]
    class PulseNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string PulseNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA2";

        [ImportingConstructor]
        public PulseNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Text = "Pulse";
            IsVisible = false;
            IsEnabled = true;
            Image = Resources.pulse;

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
