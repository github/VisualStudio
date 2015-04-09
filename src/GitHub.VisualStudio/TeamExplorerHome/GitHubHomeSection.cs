using System;
using System.ComponentModel.Composition;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio.TeamExplorerHome
{
    [TeamExplorerSection(GitHubHomeSectionId, TeamExplorerPageIds.Home, 10)]
    public class GitHubHomeSection : TeamExplorerSectionBase, IGitHubHomeSection
    {
        public const string GitHubHomeSectionId = "72008232-2104-4FA0-A189-61B0C6F91198";

        protected GitHubHomeContent View
        {
            get { return this.SectionContent as GitHubHomeContent; }
            set { this.SectionContent = value; }
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
            IsVisible = ActiveRepo != null;
            if (ActiveRepo == null)
                return;
            RepoName = ActiveRepoName;
            RepoUrl = ActiveRepoUri.ToString();
            Icon = GetIcon(true, true, true);
                
            base.RepoChanged();
        }

        static Octicon GetIcon(bool isPrivate, bool isHosted, bool isFork)
        {
            return !isHosted ? Octicon.device_desktop
                : isPrivate ? Octicon.@lock
                : isFork ? Octicon.repo_forked : Octicon.repo;
        }
    }
}