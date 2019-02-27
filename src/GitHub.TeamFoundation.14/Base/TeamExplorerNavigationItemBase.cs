using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Helpers;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.PlatformUI;
using Serilog;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerNavigationItemBase : TeamExplorerItemBase, ITeamExplorerNavigationItem2
    {
        static readonly ILogger log = LogManager.ForContext<TeamExplorerNavigationItemBase>();

        readonly Octicon octicon;

        public TeamExplorerNavigationItemBase(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder, Octicon octicon)
            : base(serviceProvider, apiFactory, holder)
        {
            this.octicon = octicon;

            IsVisible = false;
            IsEnabled = true;

            OnThemeChanged();
            VSColorTheme.ThemeChanged += _ =>
            {
                OnThemeChanged();
                Invalidate();
            };

            // Navigation items need to listen for repo change events before they're visible
            SubscribeToRepoChanges();
        }

        public override void Invalidate()
        {
            IsVisible = false;
            InvalidateAsync().Forget(log);
        }

        async Task InvalidateAsync()
        {
            var uri = ActiveRepoUri;
            var isVisible = await IsAGitHubRepo(uri);
            if (ActiveRepoUri != uri)
            {
                log.Information("Not setting button visibility because repository changed from {BeforeUrl} to {AfterUrl}", uri, ActiveRepoUri);
                return;
            }

            IsVisible = isVisible;
        }

        void OnThemeChanged()
        {
            var theme = Colors.DetectTheme();
            var dark = theme == "Dark";
            Icon = SharedResources.GetDrawingForIcon(octicon, dark ? Colors.DarkThemeNavigationItem : Colors.LightThemeNavigationItem, theme);
        }

        protected void OpenInBrowser(Lazy<IVisualStudioBrowser> browser, string endpoint)
        {
            var uri = ActiveRepoUri;
            Debug.Assert(uri != null, "OpenInBrowser: uri should never be null");
#if !DEBUG
            if (uri == null)
                return;
#endif
            var browseUrl = uri.ToRepositoryUrl().Append(endpoint);

            OpenInBrowser(browser, browseUrl);
        }

        int argbColor;
        public int ArgbColor
        {
            get { return argbColor; }
            set { argbColor = value; this.RaisePropertyChange(); }
        }

        object icon;
        public object Icon
        {
            get { return icon; }
            set { icon = value; this.RaisePropertyChange(); }
        }

        Image image;
        public Image Image
        {
            get { return image; }
            set { image = value; this.RaisePropertyChange(); }
        }
    }
}
