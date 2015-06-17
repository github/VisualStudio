using System;
using System.ComponentModel.Composition;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;
using GitHub.Services;
using GitHub.Api;
using GitHub.Primitives;
using System.Threading.Tasks;
using System.Diagnostics;

namespace GitHub.VisualStudio.TeamExplorerHome
{
    [TeamExplorerSection(GitHubHomeSectionId, TeamExplorerPageIds.Home, 10)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubHomeSection : TeamExplorerSectionBase, IGitHubHomeSection
    {
        public const string GitHubHomeSectionId = "72008232-2104-4FA0-A189-61B0C6F91198";

        [ImportingConstructor]
        public GitHubHomeSection(ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder)
            : base(apiFactory, holder)
        {
            Title = "GitHub";
            View = new GitHubHomeContent();
            View.DataContext = this;
        }

        protected async override void RepoChanged()
        {
            IsVisible = false;

            base.RepoChanged();

            IsVisible = await ShouldBeVisible();

            if (IsVisible)
            {
                RepoName = ActiveRepoName;
                RepoUrl = ActiveRepoUri.ToString();
                Icon = GetIcon(false, true, false);
                Debug.Assert(SimpleApiClient != null,
                    "If we're in this block, simpleApiClient cannot be null. It was created by UpdateStatus");
                var repo = await SimpleApiClient.GetRepository();
                Icon = GetIcon(repo.Private, true, repo.Fork);
            }
        }

        public override async void Refresh()
        {
            IsVisible = await ShouldBeVisible();
            base.Refresh();
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