using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio.Helpers;
using Microsoft.TeamFoundation.Controls;
using NullGuard;
using GitHub.Extensions;
using System.Threading;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerNavigationItemBase : TeamExplorerGitAwareItemBase, ITeamExplorerNavigationItem2, INotifyPropertySource
    {
        [AllowNull]
        public ISimpleApiClient SimpleApiClient { [return: AllowNull] get; private set; }

        readonly ISimpleApiClientFactory apiFactory;
        readonly ITeamExplorerServiceHolder holder;
        readonly SynchronizationContext syncContext;

        public TeamExplorerNavigationItemBase(ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder)
            : base()
        {
            this.apiFactory = apiFactory;
            this.holder = holder;
            syncContext = SynchronizationContext.Current;
            IsVisible = false;
            IsEnabled = true;
            SubscribeToSectionProvider();
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
            get { return image; }
            set { image = value; this.RaisePropertyChange(); }
        }

        protected virtual async void UpdateState()
        {
            var visible = await Refresh();
            IsVisible = IsEnabled = visible;
        }

        protected async Task<bool> Refresh()
        {
            bool visible = false;

            if (SimpleApiClient == null)
            {
                var uri = ActiveRepoUri;
                if (uri == null)
                    return visible;

                if (HostAddress.IsGitHubDotComUri(uri))
                    visible = true;

                SimpleApiClient = apiFactory.Create(uri);

                if (!visible)
                {
                    // enterprise probe
                    var ret = await SimpleApiClient.IsEnterprise().ConfigureAwait(false);
                    visible = (ret == EnterpriseProbeResult.Ok);
                }
            }
            return visible;
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

        protected override void RepoChanged()
        {
            SimpleApiClient = null;
            UpdateState();
            base.RepoChanged();
        }

        void SubscribeToSectionProvider()
        {
            holder.Subscribe(this, (prov) =>
            {
                syncContext.Post((p) =>
                {
                    var provider = p as IServiceProvider;
                    ServiceProvider = provider;
                    Initialize();
                }, prov);
            });
        }

        void UnsubscribeToSectionProvider()
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
                    UnsubscribeToSectionProvider();
                    disposed = true;
                }
            }
            base.Dispose(disposing);
        }
    }
}
