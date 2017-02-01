using System;
using System.Diagnostics;
using System.Drawing;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Helpers;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.PlatformUI;
using NullGuard;
using GitHub.Models;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerNavigationItemBase : TeamExplorerItemBase, ITeamExplorerNavigationItem2
    {
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

            holder.Subscribe(this, UpdateRepo);
        }

        public override async void Invalidate()
        {
            IsVisible = false;
            IsVisible = await IsAGitHubRepo();
        }

        void OnThemeChanged()
        {
            var theme = Colors.DetectTheme();
            var dark = theme == "Dark";
            Icon = SharedResources.GetDrawingForIcon(octicon, dark ? Colors.DarkThemeNavigationItem : Colors.LightThemeNavigationItem, theme);
        }

        void UpdateRepo(ILocalRepositoryModel repo)
        {
            var changed = ActiveRepo != repo;
            ActiveRepo = repo;
            RepoChanged(changed);
            Invalidate();
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
