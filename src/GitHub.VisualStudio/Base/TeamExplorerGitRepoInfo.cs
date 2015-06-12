using GitHub.Services;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using NullGuard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerGitRepoInfo : TeamExplorerItemBase, IGitAwareItem
    {
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
        public string ActiveRepoName {[return: AllowNull] get; set; }
    }
}
