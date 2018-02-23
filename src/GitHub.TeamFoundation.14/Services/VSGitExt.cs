using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using GitHub.Models;
using GitHub.Services;
using GitHub.Logging;
using GitHub.TeamFoundation.Services;
using Serilog;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;

namespace GitHub.VisualStudio.Base
{
    /// <summary>
    /// This service acts as an always available version of <see cref="IGitExt"/>.
    /// </summary>
    [Export(typeof(IVSGitExt))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSGitExt : IVSGitExt
    {
        static readonly ILogger log = LogManager.ForContext<VSGitExt>();

        readonly IGitHubServiceProvider serviceProvider;
        readonly IVSUIContext context;
        readonly ILocalRepositoryModelFactory repositoryFactory;

        IGitExt gitService;
        IReadOnlyList<ILocalRepositoryModel> activeRepositories;

        [ImportingConstructor]
        public VSGitExt(IGitHubServiceProvider serviceProvider)
            : this(serviceProvider, new VSUIContextFactory(), new LocalRepositoryModelFactory())
        {
        }

        public VSGitExt(IGitHubServiceProvider serviceProvider, IVSUIContextFactory factory, ILocalRepositoryModelFactory repositoryFactory)
        {
            this.serviceProvider = serviceProvider;
            this.repositoryFactory = repositoryFactory;

            // The IGitExt service isn't available when a TFS based solution is opened directly.
            // It will become available when moving to a Git based solution (and cause a UIContext event to fire).
            context = factory.GetUIContext(new Guid(Guids.GitSccProviderId));

            // Start with empty array until we have a chance to initialize.
            ActiveRepositories = Array.Empty<ILocalRepositoryModel>();

            if (context.IsActive && TryInitialize())
            {
                // Refresh ActiveRepositories on background thread so we don't delay startup.
                QueueRefreshActiveRepositories();
            }
            else
            {
                // If we're not in the UIContext or TryInitialize fails, have another go when the UIContext changes.
                context.UIContextChanged += ContextChanged;
                log.Debug("VSGitExt will be initialized later");
            }
        }

        void ContextChanged(object sender, VSUIContextChangedEventArgs e)
        {
            // If we're in the UIContext and TryInitialize succeeds, we can stop listening for events.
            // NOTE: this event can fire with UIContext=true in a TFS solution (not just Git).
            if (e.Activated && TryInitialize())
            {
                // Refresh ActiveRepositories on background thread so we don't delay UI context change.
                QueueRefreshActiveRepositories();
                context.UIContextChanged -= ContextChanged;
                log.Debug("Initialized VSGitExt on UIContextChanged");
            }
        }

        bool TryInitialize()
        {
            gitService = serviceProvider.GetService<IGitExt>();
            if (gitService != null)
            {
                gitService.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(gitService.ActiveRepositories))
                    {
                        QueueRefreshActiveRepositories();
                    }
                };

                log.Debug("Found IGitExt service and initialized VSGitExt");
                return true;
            }

            log.Error("Couldn't find IGitExt service");
            return false;
        }

        void QueueRefreshActiveRepositories()
        {
            // Execute tasks in sequence using thread pool (TaskScheduler.Default).
            PendingTasks = PendingTasks.ContinueWith(_ => RefreshActiveRepositories(), TaskScheduler.Default);
        }

        void RefreshActiveRepositories()
        {
            try
            {
                log.Debug(
                    "IGitExt.ActiveRepositories (#{Id}) returned {Repositories}",
                    gitService.GetHashCode(),
                    gitService?.ActiveRepositories.Select(x => x.RepositoryPath));

                ActiveRepositories = gitService?.ActiveRepositories.Select(x => repositoryFactory.Create(x.RepositoryPath)).ToList();
            }
            catch (Exception e)
            {
                log.Error(e, "Error refreshing repositories");
                ActiveRepositories = Array.Empty<ILocalRepositoryModel>();
            }
        }

        public IReadOnlyList<ILocalRepositoryModel> ActiveRepositories
        {
            get
            {
                return activeRepositories;
            }

            private set
            {
                if (value != activeRepositories)
                {
                    log.Debug("ActiveRepositories changed to {Repositories}", value?.Select(x => x.CloneUrl));
                    activeRepositories = value;
                    ActiveRepositoriesChanged?.Invoke();
                }
            }
        }

        public event Action ActiveRepositoriesChanged;

        /// <summary>
        /// Tasks that are pending execution on the thread pool.
        /// </summary>
        public Task PendingTasks { get; private set; } = Task.CompletedTask;
    }
}