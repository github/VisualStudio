using System;
using GitHub.UI;
using GitHub.VisualStudio.TeamExplorerHome;

namespace DesignTimeStyleHelper
{
    public class HomeContentDesignContext
    {
        public DesignTimeGitHubHomeSection ViewModel { get { return new DesignTimeGitHubHomeSection(); } }
    }

    public class DesignTimeGitHubHomeSection : IGitHubHomeSection
    {
        public Octicon Icon { get { return Octicon.@lock; } }

        public string RepoName { get { return "octokit/octokit.net"; }  set { } }
        public string RepoUrl { get { return "https://github.com/octokit/octokit.net"; } set { } }
    }
}
