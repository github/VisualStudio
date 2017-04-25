using System;
using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Services;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using Microsoft.TeamFoundation.Controls;
using GitHub.UI;
using GitHub.VisualStudio.UI;

namespace GitHub.VisualStudio.TeamExplorer.Home
{
    [TeamExplorerNavigationItem(PulseNavigationItemId, NavigationItemPriority.Pulse)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PulseNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string PulseNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA2";

        readonly Lazy<IVisualStudioBrowser> browser;

        [ImportingConstructor]
        public PulseNavigationItem(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory,
            Lazy<IVisualStudioBrowser> browser,
            ITeamExplorerServiceHolder holder)
            : base(serviceProvider, apiFactory, holder, Octicon.pulse)
        {
            this.browser = browser;
            Text = Resources.PulseNavigationItemText;
            ArgbColor = Colors.LightBlueNavigationItem.ToInt32();
        }

        public override void Execute()
        {
            OpenInBrowser(browser, "pulse");
            base.Execute();
        }
    }
}
