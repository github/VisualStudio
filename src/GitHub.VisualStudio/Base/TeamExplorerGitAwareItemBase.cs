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
using GitHub.Services;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerGitAwareItemBase : TeamExplorerGitRepoInfo
    {
        IGitExt gitService;
        readonly SynchronizationContext syncContext;
        UIContext gitUIContext;

        public TeamExplorerGitAwareItemBase()
        {
            syncContext = SynchronizationContext.Current;
            ActiveRepo = null;
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
            Debug.Assert(ServiceProvider != null, "UIContextChanged called before service provider is set");
            if (ServiceProvider == null)
                return;

            if (active)
                GitService = ServiceProvider.GetService<IGitExt>();
            else
                GitService = null;
            UpdateRepo();
        }

        void CheckAndUpdate(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var service = GitService;
            if (service == null)
                return;

            if (e.PropertyName == "ActiveRepositories")
            {
                syncContext.Post((o) => UpdateRepo(o as IGitRepositoryInfo), service.ActiveRepositories.FirstOrDefault());
            }
        }

        void UpdateRepo()
        {
            if (GitService != null)
                UpdateRepo(gitService.ActiveRepositories.FirstOrDefault());
            else
                UpdateRepo(null);
        }

        protected void UpdateRepo([AllowNull]IGitRepositoryInfo repo)
        {
            if (ActiveRepo.Compare(repo))
                return;

            ActiveRepo = repo;
            if (repo != null)
            {
                var gitRepo = Services.GetRepoFromIGit(repo);
                var uri  = Services.GetUriFromRepository(gitRepo);
                if (uri != null)
                {
                    var name = uri.GetRepo();
                    if (name != null)
                    {
                        ActiveRepoUri = uri;
                        ActiveRepoName = ActiveRepoUri.GetUser() + "/" + ActiveRepoUri.GetRepo();
                    }
                }
            }
            RepoChanged();
        }

        protected virtual void RepoChanged()
        {
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
    }
}
