using GitHub.UI;

namespace GitHub.VisualStudio.TeamExplorer.Home
{
    public interface IGitHubHomeSection
    {
        /// <summary>
        /// The name of the repository.
        /// </summary>
        string RepoName { get; set; }

        /// <summary>
        /// The URL to the repository.
        /// </summary>
        string RepoUrl { get; set; }

        /// <summary>
        /// The icon to show next to a repository name. It indicates whether it's private, public, a fork, etc.
        /// </summary>
        Octicon Icon { get; }
    }
}
