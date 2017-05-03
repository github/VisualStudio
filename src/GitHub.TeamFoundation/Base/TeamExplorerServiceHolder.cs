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
using System.Globalization;
using GitHub.Models;

namespace GitHub.VisualStudio.Base
{
    [Export(typeof(ITeamExplorerServiceHolder))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerServiceHolder : ITeamExplorerServiceHolder
    {
        readonly Dictionary<object, Action<ILocalRepositoryModel>> activeRepoHandlers = new Dictionary<object, Action<ILocalRepositoryModel>>();
        ILocalRepositoryModel activeRepo;
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
                GitUIContext = GitUIContext ?? UIContext.FromUIContextGuid(new Guid(Guids.GitSccProviderId));
                UIContextChanged(GitUIContext?.IsActive ?? false, false);
            }
        }

        [AllowNull]
        public ILocalRepositoryModel ActiveRepo
        {
            [return: AllowNull] get { return activeRepo; }
            private set
            {
                if (activeRepo == value)
                    return;
                if (activeRepo != null)
                    activeRepo.PropertyChanged -= ActiveRepoPropertyChanged;
                activeRepo = value;
                if (activeRepo != null)
                    activeRepo.PropertyChanged += ActiveRepoPropertyChanged;
                NotifyActiveRepo();
            }
        }

        public void Subscribe(object who, Action<ILocalRepositoryModel> handler)
        {
            bool notificationsExist;
            ILocalRepositoryModel repo;
            lock(activeRepoHandlers)
            {
                repo = ActiveRepo;
                notificationsExist = activeRepoNotified;
                if (!activeRepoHandlers.ContainsKey(who))
                    activeRepoHandlers.Add(who, handler);
                else
                    activeRepoHandlers[who] = handler;
            }

            // the repo url might have changed and we don't get notifications
            // for that, so this is a good place to refresh it in case that happened
            repo?.Refresh();

            // if the active repo hasn't changed and there's notifications queued up,
            // notify the subscriber. If the repo has changed, the set above will trigger
            // notifications so we don't have to do it here.
            if (repo == ActiveRepo && notificationsExist)
                handler(repo);
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

        public void Refresh()
        {
            GitUIContext = GitUIContext ?? UIContext.FromUIContextGuid(new Guid(Guids.GitSccProviderId));
            UIContextChanged(GitUIContext?.IsActive ?? false, true);
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
            UIContextChanged(e.Activated, false);
        }

        async void UIContextChanged(bool active, bool refresh)
        {
            Debug.Assert(ServiceProvider != null, "UIContextChanged called before service provider is set");
            if (ServiceProvider == null)
                return;

            if (active)
            {
                GitService = GitService ?? ServiceProvider.GetServiceSafe<IGitExt>();
                if (ActiveRepo == null || refresh)
                    ActiveRepo = await System.Threading.Tasks.Task.Run(() =>
                    {
                        var repos = GitService?.ActiveRepositories;
                        // Looks like this might return null after a while, for some unknown reason
                        // if it does, let's refresh the GitService instance in case something got wonky
                        // and try again. See issue #23
                        if (repos == null)
                        {
                            VsOutputLogger.WriteLine(string.Format(CultureInfo.CurrentCulture, "Error 2001: ActiveRepositories is null. GitService: '{0}'", GitService));
                            GitService = ServiceProvider?.GetServiceSafe<IGitExt>();
                            repos = GitService?.ActiveRepositories;
                            if (repos == null)
                                VsOutputLogger.WriteLine(string.Format(CultureInfo.CurrentCulture, "Error 2002: ActiveRepositories is null. GitService: '{0}'", GitService));
                        }
                        return repos?.FirstOrDefault()?.ToModel();
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

            var repo = service.ActiveRepositories.FirstOrDefault()?.ToModel();
            if (repo != ActiveRepo)
                // so annoying that this is on the wrong thread
                syncContext.Post(r => ActiveRepo = r as ILocalRepositoryModel, repo);
        }

        void ActiveRepoPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CloneUrl")
                ActiveRepo = sender as ILocalRepositoryModel;
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
            get { return ServiceProvider.GetServiceSafe<ITeamExplorerPage>(); }
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

    static class IGitRepositoryInfoExtensions
    {
        /// <summary>
        /// Create a LocalRepositoryModel from a VS git repo object
        /// </summary>
        public static ILocalRepositoryModel ToModel(this IGitRepositoryInfo repo)
        {
            return repo == null ? null : new LocalRepositoryModel(repo.RepositoryPath);
        }
    }
}
