using Microsoft.TeamFoundation.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using NullGuard;
using GitHub.VisualStudio.Base;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.TeamFoundation.Client;
using GitHub.Api;
using Microsoft.VisualStudio;
using System.Diagnostics;
using GitHub.Services;

namespace GitHub.VisualStudio
{
    public class TeamExplorerNavigationItemBase : TeamExplorerGitAwareItem, ITeamExplorerNavigationItem2, INotifyPropertySource
    {
        [AllowNull]
        public ISimpleApiClient SimpleApiClient { get; private set; }

        readonly ISimpleApiClientFactory apiFactory;

        public TeamExplorerNavigationItemBase(IServiceProvider serviceProvider, ISimpleApiClientFactory apiFactory)
            : base()
        {
            this.ServiceProvider = serviceProvider;
            this.apiFactory = apiFactory;
            Initialize();
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
            get { return icon; }
            set { icon = value; this.RaisePropertyChange(); }
        }

        Image image;
        [AllowNull]
        public Image Image
        {
            get { return image; }
            set { image = value; this.RaisePropertyChange(); }
        }

        protected async Task<bool> Refresh()
        {
            bool visible = false;

            if (SimpleApiClient == null)
            {
                var solution = ServiceProvider.GetSolution();
                var uri = Services.GetRepoUrlFromSolution(solution);
                if (uri == null)
                    return visible;

                if (HostAddress.IsGitHubDotComUri(uri))
                    visible = true;

                SimpleApiClient = apiFactory.Create(uri);

                if (!visible)
                {
                    // enterprise probe
                    var ret = await SimpleApiClient.IsEnterprise();
                    visible = (ret == EnterpriseProbeResult.Ok);
                }
            }
            return visible;
        }

        protected async void OpenInBrowser(Lazy<IBrowser> browser, string endpoint)
        {
            var b = browser.Value;
            Debug.Assert(b != null, "Could not create a browser helper instance.");
            if (b == null)
                return;

            var repo = await SimpleApiClient.GetRepository();
            var url = repo.HtmlUrl;

            Debug.Assert(!string.IsNullOrEmpty(repo.HtmlUrl), "Could not get repository information");
            if (string.IsNullOrEmpty(repo.HtmlUrl))
                return;

            var wiki = new Uri(repo.HtmlUrl + "/" + endpoint);
            b.OpenUrl(wiki);
        }


        protected override void ContextChanged(object sender, ContextChangedEventArgs e)
        {
            SimpleApiClient = null;
            base.ContextChanged(sender, e);
        }
    }
}
