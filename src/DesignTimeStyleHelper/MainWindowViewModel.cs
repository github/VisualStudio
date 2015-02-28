using GitHub.VisualStudio.TeamExplorerHome;

namespace DesignTimeStyleHelper
{
    public class HomeContentDesignContext
    {
        public DesignTimeGitHubHomeSection ViewModel { get { return new DesignTimeGitHubHomeSection(); } }
    }

    public class DesignTimeGitHubHomeSection : IGitHubHomeSection
    {
        public string RepoName { get { return "Repository: octokit/octokit.net"; }  set { } }
        public string RepoUrl { get { return "Remote: https://github.com/octokit/octokit.net"; } set { } }
    }
}
