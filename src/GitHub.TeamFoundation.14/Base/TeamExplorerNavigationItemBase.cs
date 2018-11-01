using System;
using System.Diagnostics;
using System.Drawing;
using System.ComponentModel;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Helpers;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.PlatformUI;
using GitHub.Models;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerNavigationItemBase : TeamExplorerItemBase, ITeamExplorerNavigationItem2
    {
        readonly ITeamExplorerServiceHolder holder;
        readonly Octicon octicon;

        public TeamExplorerNavigationItemBase(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder, Octicon octicon)
            : base(serviceProvider, apiFactory, holder)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            Guard.ArgumentNotNull(apiFactory, nameof(apiFactory));
            Guard.ArgumentNotNull(holder, nameof(holder));

            this.holder = holder;
            this.octicon = octicon;

            IsVisible = false;
            IsEnabled = true;

            OnThemeChanged();
            VSColorTheme.ThemeChanged += _ =>
            {
                OnThemeChanged();
                Invalidate();
            };

            ActiveRepo = holder.TeamExplorerContext.ActiveRepository;
            holder.TeamExplorerContext.PropertyChanged += TeamExplorerContext_PropertyChanged;
            holder.TeamExplorerContext.StatusChanged += TeamExplorerContext_StatusChanged;
        }

        void TeamExplorerContext_StatusChanged(object sender, EventArgs e)
        {
            UpdateRepoOnMainThread(holder.TeamExplorerContext.ActiveRepository);
        }

        void TeamExplorerContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(holder.TeamExplorerContext.ActiveRepository))
            {
                UpdateRepoOnMainThread(holder.TeamExplorerContext.ActiveRepository);
            }
        }

        void UpdateRepoOnMainThread(ILocalRepositoryModel repo)
        {
            holder.JoinableTaskFactory.RunAsync(async () =>
            {
                await holder.JoinableTaskFactory.SwitchToMainThreadAsync();
                UpdateRepo(repo);
            }).Task.Forget();
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
            holder.TeamExplorerContext.PropertyChanged -= TeamExplorerContext_PropertyChanged;
            holder.TeamExplorerContext.StatusChanged -= TeamExplorerContext_StatusChanged; ;
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
