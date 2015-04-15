using System;
using System.ComponentModel.Composition;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;
using System.Diagnostics;
using GitHub.Services;

namespace GitHub.VisualStudio.TeamExplorerHome
{
    [TeamExplorerSection(GitHubHomeSectionId, TeamExplorerPageIds.Home, 10)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubHomeSection : TeamExplorerSectionBase, IGitHubHomeSection
    {
        public const string GitHubHomeSectionId = "72008232-2104-4FA0-A189-61B0C6F91198";

        readonly ITeamExplorerServiceHolder holder;

        public GitHubHomeSection(ITeamExplorerServiceHolder holder)
            : base()
        {
            this.holder = holder;
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

        public GitHubHomeSection()
        {
            Title = "GitHub";
            // only when the repo is hosted on github.com
            IsVisible = false;
            IsExpanded = true;
            View = new GitHubHomeContent();
            View.DataContext = this;
        }

        protected override void RepoChanged()
        {
            IsVisible = ActiveRepoUri != null;
            if (ActiveRepoUri != null)
            {
                RepoName = ActiveRepoName;
                RepoUrl = ActiveRepoUri.ToString();
                Icon = GetIcon(true, true, true);
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
    }
}