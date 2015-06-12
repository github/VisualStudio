using System;
using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Services;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;
using System.Windows.Media;
using GitHub.UI;

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
            Icon = SharedResources.GetDrawingForIcon(Octicon.book, new SolidColorBrush(Color.FromRgb(66, 66, 66)));
            ArgbColor = Helpers.Colors.BlueNavigationItem.ToInt32();
        }

        public override void Execute()
        {
            OpenInBrowser(browser, "wiki");
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
