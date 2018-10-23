using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using Serilog;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System.Windows;

namespace GitHub.VisualStudio.Base
{
    [Export(typeof(ITeamExplorerServiceHolder))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerServiceHolder : ITeamExplorerServiceHolder
    {
        static readonly ILogger log = LogManager.ForContext<TeamExplorerServiceHolder>();

        readonly Dictionary<object, Action<ILocalRepositoryModel>> activeRepoHandlers = new Dictionary<object, Action<ILocalRepositoryModel>>();
        ILocalRepositoryModel activeRepo;
        bool activeRepoNotified = false;

        IServiceProvider serviceProvider;
        readonly IVSGitExt gitExt;
        readonly IGitService gitService;

        /// <summary>
        /// This class relies on IVSGitExt that provides information when VS switches repositories.
        /// </summary>
        /// <param name="gitExt">Used for monitoring the active repository.</param>
        [ImportingConstructor]
        TeamExplorerServiceHolder(IVSGitExt gitExt, IGitService gitService) : this(gitExt, gitService, ThreadHelper.JoinableTaskContext)
        {
        }

        /// <summary>
        /// This constructor can be used for unit testing.
        /// </summary>
        /// <param name="gitService">Used for monitoring the active repository.</param>
        /// <param name="joinableTaskContext">Used for switching to the Main thread.</param>
        public TeamExplorerServiceHolder(IVSGitExt gitExt, IGitService gitService, JoinableTaskContext joinableTaskContext)
        {
            JoinableTaskCollection = joinableTaskContext.CreateCollection();
            JoinableTaskCollection.DisplayName = nameof(TeamExplorerServiceHolder);
            JoinableTaskFactory = joinableTaskContext.CreateFactory(JoinableTaskCollection);

            // HACK: This might be null in SafeMode.
            // We should be throwing rather than returning a null MEF service.
            if (gitExt == null)
            {
                return;
            }

            this.gitExt = gitExt;
            this.gitService = gitService;
            UpdateActiveRepo();
            gitExt.ActiveRepositoriesChanged += UpdateActiveRepo;
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
            if (repo != null)
            {
                gitService.Refresh(repo);
            }

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
            var repo = gitExt.ActiveRepositories.FirstOrDefault();

            if (!Equals(repo, ActiveRepo))
            {
                // Fire property change events on Main thread
                JoinableTaskFactory.RunAsync(async () =>
                {
                    await JoinableTaskFactory.SwitchToMainThreadAsync();
                    ActiveRepo = repo;
                }).Task.Forget(log);
            }
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

        public JoinableTaskCollection JoinableTaskCollection { get; }
        JoinableTaskFactory JoinableTaskFactory { get; }
    }
}
