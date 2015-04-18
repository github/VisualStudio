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
    [TeamExplorerNavigationItem(WikiNavigationItemId,
        NavigationItemPriority.Wiki)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class WikiNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string WikiNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA1";

        readonly Lazy<IVisualStudioBrowser> browser;

        [ImportingConstructor]
        public WikiNavigationItem(ISimpleApiClientFactory apiFactory, Lazy<IVisualStudioBrowser> browser,
                                    ITeamExplorerServiceHolder holder)
            : base(apiFactory, holder)
        {
            this.browser = browser;
            Text = "Wiki";
            Image = Resources.book;
            ArgbColor = Colors.BlueNavigationItem.ToInt32();
        }

        public override void Execute()
        {
            OpenInBrowser(browser, "wiki");
            base.Execute();
        }

        protected override async void UpdateState()
        {
            bool visible = await Refresh().ConfigureAwait(true);
            if (visible)
            {
                var ret = await SimpleApiClient.HasWiki().ConfigureAwait(true);
                visible = (ret == WikiProbeResult.Ok);
            }
            
            IsVisible = IsEnabled = visible;
        }
    }
}
