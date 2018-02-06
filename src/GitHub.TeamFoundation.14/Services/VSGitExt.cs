using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using GitHub.Models;
using GitHub.Services;
using GitHub.Logging;
using Serilog;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using Task = System.Threading.Tasks.Task;

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

        // Services need to be imported from the current assembly (GitHub.TeamFoundation.14 or 15).
        // We therefore qualify each service with a contract name that indicates the assembly.
        // This is a fix for https://github.com/github/VisualStudio/issues/1454
#if TEAMEXPLORER14
        const string ContractName = "TEAMEXPLORER14";
#elif TEAMEXPLORER15
        const string ContractName = "TEAMEXPLORER15";
#endif

        [ImportingConstructor]
        public VSGitExt(IGitHubServiceProvider serviceProvider,
            [Import(ContractName)] IVSUIContextFactory factory,
            [Import(ContractName)] ILocalRepositoryModelFactory repositoryFactory)
        {
            this.serviceProvider = serviceProvider;
            this.repositoryFactory = repositoryFactory;

            // The IGitExt service isn't available when a TFS based solution is opened directly.
            // It will become available when moving to a Git based solution (cause a UIContext event to fire).
            context = factory.GetUIContext(new Guid(Guids.GitSccProviderId));

            // Start with empty array until we have a change to initialize.
            ActiveRepositories = Array.Empty<ILocalRepositoryModel>();

            if (context.IsActive && TryInitialize())
            {
                // Refresh ActiveRepositories on background thread so we don't delay startup.
                InitializeTask = Task.Run(() => RefreshActiveRepositories());
            }
            else
            {
                // If we're not in the UIContext or TryInitialize fails, have another go when the UIContext changes.
                context.UIContextChanged += ContextChanged;
                log.Information("VSGitExt will be initialized later");
                InitializeTask = Task.CompletedTask;
            }
        }

        [Export(ContractName, typeof(ILocalRepositoryModelFactory))]
        class LocalRepositoryModelFactory : ILocalRepositoryModelFactory
        {
            public ILocalRepositoryModel Create(string localPath)
            {
                return new LocalRepositoryModel(localPath);
            }
        }

        [Export(ContractName, typeof(IVSUIContextFactory))]
        class VSUIContextFactory : IVSUIContextFactory
        {
            public IVSUIContext GetUIContext(Guid contextGuid)
            {
                return new VSUIContext(contextGuid);
            }
        }

        void ContextChanged(object sender, UIContextChangedEventArgs e)
        {
            // If we're in the UIContext and TryInitialize succeeds, we can stop listening for events.
            // NOTE: this event can fire with UIContext=true in a TFS solution (not just Git).
            if (e.Activated && TryInitialize())
            {
                // Refresh ActiveRepositories on background thread so we don't delay UI context change.
                InitializeTask = Task.Run(() => RefreshActiveRepositories());
                context.UIContextChanged -= ContextChanged;
                log.Information("Initialized VSGitExt on UIContextChanged");
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
                        RefreshActiveRepositories();
                    }
                };

                log.Information("Found IGitExt service and initialized VSGitExt");
                return true;
            }

            log.Information("Couldn't find IGitExt service");
            return false;
        }

        void RefreshActiveRepositories()
        {
            try
            {
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
                    activeRepositories = value;
                    ActiveRepositoriesChanged?.Invoke();
                }
            }
        }

        public event Action ActiveRepositoriesChanged;

        public Task InitializeTask { get; private set; }
    }

    public interface IVSUIContextFactory
    {
        IVSUIContext GetUIContext(Guid contextGuid);
    }

    public interface IVSUIContext
    {
        bool IsActive { get; }
        event EventHandler<UIContextChangedEventArgs> UIContextChanged;
    }

    class VSUIContext : IVSUIContext
    {
        readonly UIContext context;
        readonly Dictionary<EventHandler<UIContextChangedEventArgs>, EventHandler<UIContextChangedEventArgs>> handlers =
            new Dictionary<EventHandler<UIContextChangedEventArgs>, EventHandler<UIContextChangedEventArgs>>();
        public VSUIContext(Guid contextGuid)
        {
            context = UIContext.FromUIContextGuid(contextGuid);
        }

        public bool IsActive { get { return context.IsActive; } }

        public event EventHandler<UIContextChangedEventArgs> UIContextChanged
        {
            add
            {
                EventHandler<UIContextChangedEventArgs> handler = null;
                if (!handlers.TryGetValue(value, out handler))
                {
                    handlers.Add(value, handler);
                }
                context.UIContextChanged += handler;
            }
            remove
            {
                EventHandler<UIContextChangedEventArgs> handler = null;
                if (handlers.TryGetValue(value, out handler))
                {
                    handlers.Remove(value);
                    context.UIContextChanged -= handler;
                }
            }
        }
    }
}