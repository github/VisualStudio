using System;
using System.Diagnostics;
using GitHub.Services;
using Microsoft.TeamFoundation.Controls;
using NullGuard;
using GitHub.Extensions;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using System.Linq;
using System.Threading;

namespace GitHub.VisualStudio.Base
{
    [Export(typeof(ITeamExplorerServiceHolder))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerServiceHolder : ITeamExplorerServiceHolder
    {
        readonly Dictionary<object, Action<IGitRepositoryInfo>> activeRepoHandlers = new Dictionary<object, Action<IGitRepositoryInfo>>();
        IGitRepositoryInfo activeRepo;
        bool activeRepoNotified = false;

        IServiceProvider serviceProvider;
        IGitExt gitService;
        UIContext gitUIContext;

        // ActiveRepositories PropertyChanged event comes in on a non-main thread
        readonly SynchronizationContext syncContext;

        public TeamExplorerServiceHolder()
        {
            syncContext = SynchronizationContext.Current;
        }

        // set by the sections when they get initialized
        [AllowNull]
        public IServiceProvider ServiceProvider
        {
            [return: AllowNull] get { return serviceProvider; }
            set
            {
                if (serviceProvider == value)
                    return;

                serviceProvider = value;
                if (serviceProvider == null)
                    return;
                GitUIContext = GitUIContext ?? UIContext.FromUIContextGuid(new Guid("11B8E6D7-C08B-4385-B321-321078CDD1F8"));
                UIContextChanged(GitUIContext?.IsActive ?? false);
            }
        }

        [AllowNull]
        public IGitRepositoryInfo ActiveRepo
        {
            [return: AllowNull] get { return activeRepo; }
            private set
            {
                if (activeRepo.Compare(value))
                    return;
                activeRepo = value;
                NotifyActiveRepo();
            }
        }

        public void Subscribe(object who, Action<IGitRepositoryInfo> handler)
        {
            lock(activeRepoHandlers)
            {
                var repo = ActiveRepo;
                if (!activeRepoHandlers.ContainsKey(who))
                    activeRepoHandlers.Add(who, handler);
                else
                    activeRepoHandlers[who] = handler;
                if (activeRepoNotified)
                    handler(repo);
            }
        }

        public void Unsubscribe(object who)
        {
            if (activeRepoHandlers.ContainsKey(who))
                activeRepoHandlers.Remove(who);
        }

        /// <summary>
        /// Clears the current ServiceProvider if it matches the one that is passed in.
        /// This is usually called on Dispose, which might happen after another section
        /// has changed the ServiceProvider to something else, which is why we require
        /// the parameter to match.
        /// </summary>
        /// <param name="provider">If the current ServiceProvider matches this, clear it</param>
        public void ClearServiceProvider(IServiceProvider provider)
        {
            if (serviceProvider != provider)
                return;

            ServiceProvider = null;
        }

        void NotifyActiveRepo()
        {
            lock (activeRepoHandlers)
            {
                activeRepoNotified = true;
                foreach (var handler in activeRepoHandlers.Values)
                    handler(activeRepo);
            }
        }

        void UIContextChanged(object sender, UIContextChangedEventArgs e)
        {
            ActiveRepo = null;
            UIContextChanged(e.Activated);
        }

        async void UIContextChanged(bool active)
        {
            Debug.Assert(ServiceProvider != null, "UIContextChanged called before service provider is set");
            if (ServiceProvider == null)
                return;

            if (active)
            {
                GitService = GitService ?? ServiceProvider.GetService<IGitExt>();
                if (ActiveRepo == null)
                    ActiveRepo = await System.Threading.Tasks.Task.Run(() =>
                    {
                        var repos = GitService?.ActiveRepositories;
                        // Looks like this might return null after a while, for some unknown reason
                        // if it does, let's refresh the GitService instance in case something got wonky
                        // and try again. See issue #23
                        if (repos == null)
                        {
                            GitService = ServiceProvider?.GetService<IGitExt>();
                            repos = GitService?.ActiveRepositories;
                        }
                        return repos?.FirstOrDefault();
                    });
            }
            else
                ActiveRepo = null;
        }

        void CheckAndUpdate(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "ActiveRepositories")
                return;

            var service = GitService;
            if (service == null)
                return;

            var repo = service.ActiveRepositories.FirstOrDefault();
            // this comparison is safe, the extension method supports null instances
            if (!repo.Compare(ActiveRepo))
                // so annoying that this is on the wrong thread
                syncContext.Post(r => ActiveRepo = r as IGitRepositoryInfo, repo);
        }

        public IGitAwareItem HomeSection
        {
            [return:AllowNull]
            get
            {
                if (ServiceProvider == null)
                    return null;
                var page = PageService;
                if (page == null)
                    return null;
                return page.GetSection(new Guid(TeamExplorer.Home.GitHubHomeSection.GitHubHomeSectionId)) as IGitAwareItem;
            }
        }

        ITeamExplorerPage PageService
        {
            [return:AllowNull]
            get { return ServiceProvider.GetService<ITeamExplorerPage>(); }
        }

        [AllowNull]
        UIContext GitUIContext
        {
            [return: AllowNull]
            get { return gitUIContext; }
            set
            {
                if (gitUIContext == value)
                    return;
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
                if (gitService == value)
                    return;
                if (gitService != null)
                    gitService.PropertyChanged -= CheckAndUpdate;
                gitService = value;
                if (gitService != null)
                    gitService.PropertyChanged += CheckAndUpdate;
            }
        }
    }
}
