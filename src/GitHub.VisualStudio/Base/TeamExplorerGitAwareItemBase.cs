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
using Microsoft.TeamFoundation.Controls;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerGitAwareItemBase : TeamExplorerItemBase
    {
        IGitRepositoryInfo activeRepo;
        IGitExt gitService;
        Uri activeRepoUri;
        string activeRepoName = string.Empty;
        SynchronizationContext syncContext;
        private UIContext gitUIContext;

        public TeamExplorerGitAwareItemBase()
        {
            syncContext = SynchronizationContext.Current;
        }

        protected virtual void RepoChanged()
        {
        }

        protected void Initialize()
        {
            if (ServiceProvider != null)
            {
                if (GitUIContext == null)
                    GitUIContext = UIContext.FromUIContextGuid(new Guid("11B8E6D7-C08B-4385-B321-321078CDD1F8"));
                UIContextChanged(GitUIContext.IsActive);
            }
        }

        void UIContextChanged(object sender, UIContextChangedEventArgs e)
        {
            UIContextChanged(e.Activated);
        }

        void UIContextChanged(bool active)
        {
            if (active)
                GitService = ServiceProvider.GetService<IGitExt>();
            else
                GitService = null;
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
            if (GitService != null)
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

        bool disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    GitUIContext = null;
                    GitService = null;
                    disposed = true;
                }
            }
            base.Dispose(disposing);
        }

        [AllowNull]
        UIContext GitUIContext
        {
            [return: AllowNull]
            get
            { return gitUIContext; }
            set
            {
                if (gitUIContext != null)
                    gitUIContext.UIContextChanged -= UIContextChanged;
                gitUIContext = value;
                if (gitUIContext != null)
                    gitUIContext.UIContextChanged += UIContextChanged;
            }
        }

        [AllowNull]
        IGitExt GitService
        {
            [return: AllowNull]
            get { return gitService; }
            set
            {
                if (gitService != null)
                    gitService.PropertyChanged -= CheckAndUpdate;
                gitService = value;
                if (gitService != null)
                    gitService.PropertyChanged += CheckAndUpdate;
            }
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
