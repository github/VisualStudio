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
    [TeamExplorerNavigationItem(WikiNavigationItemId,
        NavigationItemPriority.Wiki,
        TargetPageId = TeamExplorerPageIds.Home)]
    class WikiNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string WikiNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA1";

        [Import(typeof(IBrowser))]
        Lazy<IBrowser> browser;

        [ImportingConstructor]
        public WikiNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory)
            : base(serviceProvider, apiFactory)
        {
            Text = "Wiki";
            IsVisible = false;
            IsEnabled = false;
            Image = Resources.book;
            ArgbColor = Colors.BlueNavigationItem.ToInt32();

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
            var wiki = new Uri(repo.HtmlUrl + "/wiki");
            b.OpenUrl(wiki);
            base.Execute();
        }

        protected override async void UpdateState()
        {
            bool visible = await Refresh();
            if (visible)
            {
                var ret = await SimpleApiClient.HasWiki();
                visible = (ret == WikiProbeResult.Ok);
            }
            
            IsVisible = IsEnabled = visible;
        }
    }
}
