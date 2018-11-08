using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio.Helpers;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerGitRepoInfo : TeamExplorerBase, IGitAwareItem
    {
        public TeamExplorerGitRepoInfo(IGitHubServiceProvider serviceProvider) : base(serviceProvider)
        {
            activeRepo = null;
            activeRepoUri = null;
            activeRepoName = string.Empty;
        }

        LocalRepositoryModel activeRepo;
        public LocalRepositoryModel ActiveRepo
        {
            get { return activeRepo; }
            set
            {
                ActiveRepoName = string.Empty;
                ActiveRepoUri = null;
                activeRepo = value;
                this.RaisePropertyChange();
            }
        }

        UriString activeRepoUri;
        /// <summary>
        /// Represents the web URL of the repository on GitHub.com, even if the origin is an SSH address.
        /// </summary>
        public UriString ActiveRepoUri
        {
            get { return activeRepoUri; }
            set { activeRepoUri = value; this.RaisePropertyChange(); }
        }

        string activeRepoName;
        public string ActiveRepoName
        {
            get { return activeRepoName; }
            set { activeRepoName = value; this.RaisePropertyChange(); }
        }
    }
}
