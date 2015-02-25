using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;
using NullGuard;
using System;
using Microsoft.TeamFoundation.Client;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio
{
    [TeamExplorerSection(GitHubHomeSectionId, TeamExplorerPageIds.Home, 10)]
    public class GitHubHomeSection : TeamExplorerSectionBase
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
            get { return "Repository: " + repoName; }
            set { repoName = value; this.RaisePropertyChange(); }
        }

        string repoUrl = String.Empty;
        public string RepoUrl
        {
            get { return "Remote: " + repoUrl; }
            set { repoUrl = value; this.RaisePropertyChange(); }
        }

        [ImportingConstructor]
        public GitHubHomeSection([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            :  base(serviceProvider)
        {
            Title = "GitHub";
            // only when the repo is hosted on github.com
            IsVisible = false;
            IsExpanded = true;
            View = new GitHubHomeContent();
            View.ViewModel = this;
        }

        protected override void ContextChanged(object sender, ContextChangedEventArgs e)
        {
            base.ContextChanged(sender, e);
            if (e.NewContext != null && e.NewContext.HasTeamProject)
            {
                RepoName = e.NewContext.TeamProjectName;
                RepoUrl = e.NewContext.TeamProjectUri.ToString();
            }
        }
    }
}