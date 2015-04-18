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
        NavigationItemPriority.Graphs)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GraphsNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string GraphsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA5";

        readonly Lazy<IVisualStudioBrowser> browser;

        [ImportingConstructor]
        public GraphsNavigationItem(ISimpleApiClientFactory apiFactory, Lazy<IVisualStudioBrowser> browser,
                                    ITeamExplorerServiceHolder holder)
            : base(apiFactory, holder)
        {
            this.browser = browser;
            Text = "Graphs";
            Image = Resources.graph;
            ArgbColor = Colors.LightBlueNavigationItem.ToInt32();
        }

        protected override void ContextChanged(object sender, ContextChangedEventArgs e)
        {
            UpdateState();
            base.ContextChanged(sender, e);
        }

        protected override void RepoChanged()
        {
            UpdateState();
            base.RepoChanged();
        }

        protected async override void UpdateState()
        {
            IsVisible = IsEnabled = await Refresh().ConfigureAwait(true);
        }

        public override void Execute()
        {
            OpenInBrowser(browser, "graphs");
            base.Execute();
        }
    }
}
