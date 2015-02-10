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
    [TeamExplorerNavigationItem(GraphsNavigationItemId,
        NavigationItemPriority.Graphs,
        TargetPageId = TeamExplorerPageIds.Home)]
    class GraphsNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string GraphsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA5";

        [Import(typeof(IBrowser))]
        Lazy<IBrowser> browser;

        [ImportingConstructor]
        public GraphsNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory)
            : base(serviceProvider, apiFactory)
        {
            Text = "Graphs";
            IsVisible = false;
            IsEnabled = false;
            Image = Resources.graph;
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
            var wiki = new Uri(repo.HtmlUrl + "/graphs");
            b.OpenUrl(wiki);
            base.Execute();
        }

        protected override async void UpdateState()
        {
            IsVisible = IsEnabled = await Refresh();
        }
    }
}
