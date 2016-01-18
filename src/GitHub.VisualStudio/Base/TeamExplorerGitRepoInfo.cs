using GitHub.Api;
using GitHub.Services;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using NullGuard;
using System;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerGitRepoInfo : TeamExplorerBase, IGitAwareItem
    {
        public TeamExplorerGitRepoInfo()
        {
            ActiveRepo = null;
        }

        IGitRepositoryInfo activeRepo;
        [AllowNull]
        public IGitRepositoryInfo ActiveRepo
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

        [AllowNull]
        public Uri ActiveRepoUri { [return: AllowNull] get; set; }
        public string ActiveRepoName { get; set; }
    }
}
