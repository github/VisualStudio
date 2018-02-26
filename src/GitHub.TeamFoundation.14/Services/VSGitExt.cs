using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using GitHub.Models;
using GitHub.Services;
using GitHub.Logging;
using GitHub.Helpers;
using GitHub.TeamFoundation.Services;
using Serilog;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;

namespace GitHub.VisualStudio.Base
{
    /// <summary>
    /// This service acts as an always available version of <see cref="IGitExt"/>.
    /// </summary>
    /// <remarks>
    /// Initialization for this service will be done asynchronously and the <see cref="IGitExt" /> service will be
    /// retrieved on the Main thread. This means the service to be constructed and subscribed to on a background thread.
    /// </remarks>
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

            PendingTasks = InitializeAsync();
        }

        async Task InitializeAsync()
        {
            try
            {
                if (!context.IsActive || !await TryInitialize())
                {
                    // If we're not in the UIContext or TryInitialize fails, have another go when the UIContext changes.
                    context.UIContextChanged += ContextChanged;
                    log.Debug("VSGitExt will be initialized later");
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Initializing");
            }
        }

        void ContextChanged(object sender, VSUIContextChangedEventArgs e)
        {
            if (e.Activated)
            {
                PendingTasks = ContextChangedAsync();
            }
        }

        async Task ContextChangedAsync()
        {
            try
            {
                // If we're in the UIContext and TryInitialize succeeds, we can stop listening for events.
                // NOTE: this event can fire with UIContext=true in a TFS solution (not just Git).
                if (await TryInitialize())
                {
                    context.UIContextChanged -= ContextChanged;
                    log.Debug("Initialized VSGitExt on UIContextChanged");
                }
            }
            catch (Exception e)
            {
                log.Error(e, "UIContextChanged");
            }
        }

        async Task<bool> TryInitialize()
        {
            gitService = await GetServiceAsync<IGitExt>();
            if (gitService != null)
            {
                gitService.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(gitService.ActiveRepositories))
                    {
                        // Execute tasks in sequence using thread pool (TaskScheduler.Default).
                        PendingTasks = PendingTasks.ContinueWith(_ => RefreshActiveRepositories(), TaskScheduler.Default);
                    }
                };

                // Do this after we start listening so we don't miss an event.
                await Task.Run(() => RefreshActiveRepositories());

                log.Debug("Found IGitExt service and initialized VSGitExt");
                return true;
            }

            log.Error("Couldn't find IGitExt service");
            return false;
        }

        async Task<T> GetServiceAsync<T>() where T : class
        {
            // GetService must be called from the Main thread.
            await ThreadingHelper.SwitchToMainThreadAsync();
            return serviceProvider.GetService<T>();
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
        public Task PendingTasks { get; private set; }
    }
}