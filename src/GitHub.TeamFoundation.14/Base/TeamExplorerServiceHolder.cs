using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using Microsoft.TeamFoundation.Controls;

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
        readonly IVSGitExt gitService;

        // ActiveRepositories PropertyChanged event comes in on a non-main thread
        readonly SynchronizationContext syncContext;

        /// <summary>
        /// This class relies on IVSGitExt that provides information when VS switches repositories.
        /// </summary>
        /// <param name="gitService">Used for monitoring the active repository.</param>
        [ImportingConstructor]
        public TeamExplorerServiceHolder(IVSGitExt gitService)
        {
            this.gitService = gitService;
            syncContext = SynchronizationContext.Current;

            UpdateActiveRepo();
            if (gitService != null)
            {
                gitService.ActiveRepositoriesChanged += UpdateActiveRepo;
            }
        }


        // set by the sections when they get initialized
        public IServiceProvider ServiceProvider
        {
            get { return serviceProvider; }
            set
            {
                if (serviceProvider == value)
                    return;

                serviceProvider = value;
                if (serviceProvider == null)
                    return;
            }
        }

        public ILocalRepositoryModel ActiveRepo
        {
            get { return activeRepo; }
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
            Guard.ArgumentNotNull(who, nameof(who));
            Guard.ArgumentNotNull(handler, nameof(handler));

            bool notificationsExist;
            ILocalRepositoryModel repo;
            lock (activeRepoHandlers)
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
            Guard.ArgumentNotNull(who, nameof(who));

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
            Guard.ArgumentNotNull(provider, nameof(provider));

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

        void UpdateActiveRepo()
        {
            // NOTE: gitService might be null in Blend or Safe Mode
            var repo = gitService?.ActiveRepositories.FirstOrDefault();

            if (!Equals(repo, ActiveRepo))
                // so annoying that this is on the wrong thread
                syncContext.Post(r => ActiveRepo = r as ILocalRepositoryModel, repo);
        }

        void ActiveRepoPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Guard.ArgumentNotNull(e, nameof(e));

            if (e.PropertyName == "CloneUrl")
                ActiveRepo = sender as ILocalRepositoryModel;
        }

        public IGitAwareItem HomeSection
        {
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
            get { return ServiceProvider.GetServiceSafe<ITeamExplorerPage>(); }
        }
    }
}
