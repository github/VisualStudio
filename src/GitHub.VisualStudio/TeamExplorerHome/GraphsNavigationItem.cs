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
    [TeamExplorerNavigationItem(GraphsNavigationItemId,
        NavigationItemPriority.Graphs,
        TargetPageId = TeamExplorerPageIds.Home)]
    public class GraphsNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string GraphsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA5";

        readonly Lazy<IVisualStudioBrowser> browser;

        [ImportingConstructor]
        public GraphsNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory, Lazy<IVisualStudioBrowser> browser)
            : base(serviceProvider, apiFactory)
        {
            this.browser = browser;
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

        public override void Execute()
        {
            OpenInBrowser(browser, "graphs");
            base.Execute();
        }

        async void UpdateState()
        {
            IsVisible = IsEnabled = await Refresh();
        }
    }
}
