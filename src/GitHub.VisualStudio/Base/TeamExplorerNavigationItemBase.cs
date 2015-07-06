using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio.Helpers;
using Microsoft.TeamFoundation.Controls;
using NullGuard;
using GitHub.Extensions;
using System.Threading;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using GitHub.UI;
using Microsoft.VisualStudio.PlatformUI;
using System.Drawing;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerNavigationItemBase : TeamExplorerItemBase, ITeamExplorerNavigationItem2, INotifyPropertySource
    {
        Octicon octicon;

        public TeamExplorerNavigationItemBase(ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder, Octicon octicon)
            : base(apiFactory, holder)
        {
            this.octicon = octicon;

            IsVisible = false;
            IsEnabled = true;

            OnThemeChanged();
            VSColorTheme.ThemeChanged += _ =>
            {
                OnThemeChanged();
            };

            holder.Subscribe(this, UpdateRepo);
        }

        public override async void Invalidate()
        {
            IsVisible = false;
            IsVisible = await IsAGitHubRepo();
        }

        void OnThemeChanged()
        {
            var color = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
            var brightness = color.GetBrightness();
            var dark = brightness > 0.5f;

            Icon = SharedResources.GetDrawingForIcon(octicon, dark ? Colors.DarkThemeNavigationItem : Colors.LightThemeNavigationItem, dark ? "dark" : "light");
        }

        protected override void RepoChanged()
        {
            var home = holder.HomeSection;
            Debug.Assert(home != null, "Notifications should only happen when the home section is alive");
            if (home == null)
                ActiveRepo = null;
            base.RepoChanged();
        }

        void UpdateRepo(IGitRepositoryInfo repo)
        {
            ActiveRepo = repo;
            RepoChanged();
            Invalidate();
        }

        protected void OpenInBrowser(Lazy<IVisualStudioBrowser> browser, string endpoint)
        {
            var uri = ActiveRepoUri;
            Debug.Assert(uri != null, "OpenInBrowser: uri should never be null");
            if (uri == null)
                return;

            var https = uri.ToHttps();
            if (https == null)
                return;

            if (!Uri.TryCreate(https.ToString() + "/" + endpoint, UriKind.Absolute, out uri))
                return;

            OpenInBrowser(browser, uri);
        }

        void Unsubscribe()
        {
            holder.Unsubscribe(this);
        }

        bool disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    Unsubscribe();
                    disposed = true;
                }
            }
            base.Dispose(disposing);
        }

        int argbColor;
        public int ArgbColor
        {
            get { return argbColor; }
            set { argbColor = value; this.RaisePropertyChange(); }
        }

        object icon;
        [AllowNull]
        public object Icon
        {
            [return: AllowNull]
            get { return icon; }
            set { icon = value; this.RaisePropertyChange(); }
        }

        Image image;
        [AllowNull]
        public Image Image
        {
            [return: AllowNull]
            get{ return image; }
            set { image = value; this.RaisePropertyChange(); }
        }
    }
}
