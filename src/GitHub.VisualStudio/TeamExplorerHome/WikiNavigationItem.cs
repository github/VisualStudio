using System;
using System.ComponentModel.Composition;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio
{
    [TeamExplorerNavigationItem(WikiNavigationItemId,
        NavigationItemPriority.Wiki,
        TargetPageId = TeamExplorerPageIds.Home)]
    class WikiNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string WikiNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA1";

        [ImportingConstructor]
        public WikiNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Text = "Wiki";
            IsVisible = false;
            IsEnabled = true;
            Image = Resources.book;

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
