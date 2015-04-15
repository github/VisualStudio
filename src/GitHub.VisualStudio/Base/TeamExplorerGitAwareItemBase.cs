using System;
using GitHub.Extensions;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using Microsoft.TeamFoundation.Client;
using NullGuard;
using System.Linq;
using GitHub.VisualStudio.Helpers;
using System.Threading;
using System.Diagnostics;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerGitAwareItemBase : TeamExplorerItemBase
    {
        IGitRepositoryInfo activeRepo;
        IGitExt gitService;
        bool disposed;
        Uri activeRepoUri;
        string activeRepoName = string.Empty;
        SynchronizationContext syncContext;

        public TeamExplorerGitAwareItemBase()
        {
            syncContext = SynchronizationContext.Current;
        }

        protected virtual void RepoChanged()
        {
        }

        protected void Initialize()
        {
            UpdateRepo();
        }

        protected override void ContextChanged(object sender, ContextChangedEventArgs e)
        {
            base.ContextChanged(sender, e);
            UpdateRepo();
        }

        void CheckAndUpdate(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveRepositories")
            {
                syncContext.Post((o) => UpdateRepo(o as IGitRepositoryInfo), gitService.ActiveRepositories.FirstOrDefault());
            }
        }

        void UpdateRepo()
        {
            if (gitService == null)
            {
                var gitProviderUIContext = UIContext.FromUIContextGuid(new Guid("11B8E6D7-C08B-4385-B321-321078CDD1F8"));
                if (gitProviderUIContext.IsActive)
                {
                    Debug.Assert(ServiceProvider != null, "ServiceProvider must be set before subscribing to git changes");
                    gitService = ServiceProvider.GetService<IGitExt>();
                    gitService.PropertyChanged += CheckAndUpdate;
                }
            }

            if (gitService != null)
                UpdateRepo(gitService.ActiveRepositories.FirstOrDefault());
            else
                UpdateRepo(null);
        }

        void UpdateRepo([AllowNull]IGitRepositoryInfo repo)
        {
            if (ActiveRepo == repo)
                return;

            ActiveRepo = repo;
            if (repo != null)
            {
                var gitRepo = Services.GetRepoFromIGit(repo);
                var uri  = Services.GetUriFromRepository(gitRepo);
                if (uri != null)
                {
                    ActiveRepoUri = uri;
                    ActiveRepoName = activeRepoUri.GetUser() + "/" + activeRepoUri.GetRepo();
                }
            }
            RepoChanged();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposed)
                return;

            if (disposing)
            {
                if (gitService != null)
                    gitService.PropertyChanged -= CheckAndUpdate;
            }
            disposed = true;
        }

        [AllowNull]
        protected IGitRepositoryInfo ActiveRepo
        {
            [return: AllowNull]
            get { return activeRepo; }
            private set
            {
                activeRepoName = string.Empty;
                activeRepoUri = null;
                activeRepo = value;
            }
        }

        [AllowNull]
        protected Uri ActiveRepoUri
        {
            [return: AllowNull]
            get
            { return activeRepoUri; }
            private set { activeRepoUri = value; }
        }

        protected string ActiveRepoName
        {
            get { return activeRepoName; }
            private set { activeRepoName = value; }
        }
    }
}
