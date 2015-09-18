using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using NullGuard;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerGitRepoInfo : TeamExplorerBase, IGitAwareItem
    {
        public TeamExplorerGitRepoInfo()
        {
            ActiveRepo = null;
        }

        ISimpleRepositoryModel activeRepo;
        [AllowNull]
        public ISimpleRepositoryModel ActiveRepo
        {
            [return: AllowNull]
            get { return activeRepo; }
            set
            {
                ActiveRepoName = string.Empty;
                ActiveRepoUri = null;
                activeRepo = value;
            }
        }

        /// <summary>
        /// Represents the web URL of the repository on GitHub.com, even if the origin is an SSH address.
        /// </summary>
        [AllowNull]
        public UriString ActiveRepoUri { [return: AllowNull] get; set; }
        public string ActiveRepoName { get; set; }
    }
}
