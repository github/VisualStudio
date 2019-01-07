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
    [TeamExplorerNavigationItem(WikiNavigationItemId, NavigationItemPriority.Wiki)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class WikiNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string WikiNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA1";

        readonly Lazy<IVisualStudioBrowser> browser;

        [ImportingConstructor]
        public WikiNavigationItem(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory,
            Lazy<IVisualStudioBrowser> browser,
            ITeamExplorerServiceHolder holder)
            : base(serviceProvider, apiFactory, holder, Octicon.book)
        {
            this.browser = browser;
            Text = Resources.WikiNavigationItemText;
            ArgbColor = Colors.BlueNavigationItem.ToInt32();
        }

        public override void Execute()
        {
            OpenInBrowser(browser, "wiki");
            base.Execute();
        }

        public override async void Invalidate()
        {
            var visible = await IsAGitHubRepo(ActiveRepoUri);
            if (visible)
            {
                var repo = await SimpleApiClient.GetRepository();
                visible = repo.HasWiki && SimpleApiClient.HasWiki();
            }
            IsVisible = visible;
        }
    }
}
