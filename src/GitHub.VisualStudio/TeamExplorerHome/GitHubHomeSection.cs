using System;
using System.ComponentModel.Composition;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;
using System.Diagnostics;
using GitHub.Services;
using GitHub.Api;
using GitHub.Primitives;
using System.Threading.Tasks;

namespace GitHub.VisualStudio.TeamExplorerHome
{
    [TeamExplorerSection(GitHubHomeSectionId, TeamExplorerPageIds.Home, 10)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubHomeSection : TeamExplorerSectionBase, IGitHubHomeSection
    {
        public const string GitHubHomeSectionId = "72008232-2104-4FA0-A189-61B0C6F91198";

        readonly ISimpleApiClientFactory apiFactory;
        readonly ITeamExplorerServiceHolder holder;
        ISimpleApiClient simpleApiClient;

        [ImportingConstructor]
        public GitHubHomeSection(ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder)
            : base()
        {
            this.apiFactory = apiFactory;
            this.holder = holder;
            Title = "GitHub";
            IsVisible = false;
            IsExpanded = true;
            View = new GitHubHomeContent();
            View.DataContext = this;
        }

        protected async override void RepoChanged()
        {
            simpleApiClient = null;
            var visible = await UpdateState().ConfigureAwait(true);

            if (visible)
            {
                RepoName = ActiveRepoName;
                RepoUrl = ActiveRepoUri.ToString();
                IsVisible = IsEnabled = visible;
                Icon = GetIcon(false, true, false);
                var repo = await simpleApiClient.GetRepository();
                Icon = GetIcon(repo.Private, true, repo.Fork);
            }

            base.RepoChanged();
        }

        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);
            Debug.Assert(holder != null, "Could not get an instance of TeamExplorerServiceHolder");
            if (holder == null)
                return;
            holder.SetServiceProvider(e.ServiceProvider);
        }

        async Task<bool> UpdateState()
        {
            bool visible = false;

            if (simpleApiClient == null)
            {
                var uri = ActiveRepoUri;
                if (uri == null)
                    return false;

                simpleApiClient = apiFactory.Create(uri);

                if (HostAddress.IsGitHubDotComUri(uri))
                    visible = true;

                if (!visible)
                {
                    // enterprise probe
                    var ret = await simpleApiClient.IsEnterprise().ConfigureAwait(true);
                    visible = (ret == EnterpriseProbeResult.Ok);
                }
            }

            return visible;
        }

        public override void Loaded(object sender, SectionLoadedEventArgs e)
        {
            base.Loaded(sender, e);
            holder.Notify();
        }

        bool disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                holder.ClearServiceProvider(ServiceProvider);
                disposed = true;
            }
            base.Dispose(disposing);
        }

        static Octicon GetIcon(bool isPrivate, bool isHosted, bool isFork)
        {
            return !isHosted ? Octicon.device_desktop
                : isPrivate ? Octicon.@lock
                : isFork ? Octicon.repo_forked : Octicon.repo;
        }

        protected GitHubHomeContent View
        {
            get { return SectionContent as GitHubHomeContent; }
            set { SectionContent = value; }
        }

        string repoName = String.Empty;
        public string RepoName
        {
            get { return repoName; }
            set { repoName = value; this.RaisePropertyChange(); }
        }

        string repoUrl = String.Empty;
        public string RepoUrl
        {
            get { return repoUrl; }
            set { repoUrl = value; this.RaisePropertyChange(); }
        }

        Octicon icon;
        public Octicon Icon
        {
            get { return icon; }
            set { icon = value; this.RaisePropertyChange(); }
        }
    }
}