using System;
using GitHub.Extensions;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using Microsoft.TeamFoundation.Client;
using NullGuard;
using System.Linq;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerGitAwareItemBase : TeamExplorerItemBase
    {
        protected IGitRepositoryInfo activeRepo;
        IGitExt gitService;
        bool disposed;

        protected virtual void Initialize()
        {
            GetGitActiveRepo();
        }

        protected void GetGitActiveRepo()
        {
            var gitProviderUIContext = UIContext.FromUIContextGuid(new Guid("11B8E6D7-C08B-4385-B321-321078CDD1F8"));
            if (gitProviderUIContext.IsActive)
            {
                gitService = ServiceProvider.GetService<IGitExt>();
                activeRepo = gitService.ActiveRepositories.FirstOrDefault();
                gitService.PropertyChanged += CheckAndUpdate;
            }
        }

        void CheckAndUpdate(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveRepositories")
            {
                UpdateRepo(gitService.ActiveRepositories.FirstOrDefault());
            }
        }

        void UpdateRepo([AllowNull]IGitRepositoryInfo repo)
        {
            activeRepo = repo;
            var tc = new TeamContext();
            if (activeRepo != null)
            {
                var gitRepo = Services.GetRepoFromIGit(activeRepo);
                tc.TeamProjectUri = Services.GetUriFromRepository(gitRepo);
                if (tc.TeamProjectUri != null)
                {
                    tc.TeamProjectName = tc.TeamProjectUri.GetUser() + "/" + tc.TeamProjectUri.GetRepo();
                    tc.HasTeamProject = true;
                }
            }
            ContextChanged(this, new ContextChangedEventArgs(CurrentContext, tc, false, true, false));
        }

        protected override void ContextChanged(object sender, ContextChangedEventArgs e)
        {
            if (gitService == null)
            {
                GetGitActiveRepo();
            }
            base.ContextChanged(sender, e);
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
    }

    class TeamContext : ITeamFoundationContext
    {
        public bool HasCollection { get; set; }
        public bool HasTeam { get; set; }
        public bool HasTeamProject { get; set; }
        public Guid TeamId { get; set; }
        public string TeamName { get; set; }
        public TfsTeamProjectCollection TeamProjectCollection { get; set; }
        [AllowNull]
        public string TeamProjectName { get; set; }
        [AllowNull]
        public Uri TeamProjectUri { get; set; }
    }

}
