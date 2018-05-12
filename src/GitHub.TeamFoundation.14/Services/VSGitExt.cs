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
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using Task = System.Threading.Tasks.Task;

namespace GitHub.VisualStudio.Base
{
    /// <summary>
    /// This service acts as an always available version of <see cref="IGitExt"/>.
    /// </summary>
    /// <remarks>
    /// Initialization for this service will be done asynchronously and the <see cref="IGitExt" /> service will be
    /// retrieved using <see cref="GetServiceAsync" />. This means the service can be constructed and subscribed to from a background thread.
    /// </remarks>
    public class VSGitExt : IVSGitExt
    {
        static readonly ILogger log = LogManager.ForContext<VSGitExt>();

        readonly IAsyncServiceProvider asyncServiceProvider;
        readonly ILocalRepositoryModelFactory repositoryFactory;
        readonly object refreshLock = new object();

        IGitExt gitService;
        IReadOnlyList<ILocalRepositoryModel> activeRepositories;

        [ImportingConstructor]
        public VSGitExt(IAsyncServiceProvider asyncServiceProvider)
            : this(asyncServiceProvider, new VSUIContextFactory(), new LocalRepositoryModelFactory())
        {
        }

        public VSGitExt(IAsyncServiceProvider asyncServiceProvider, IVSUIContextFactory factory, ILocalRepositoryModelFactory repositoryFactory)
        {
            this.asyncServiceProvider = asyncServiceProvider;
            this.repositoryFactory = repositoryFactory;

            // Start with empty array until we have a chance to initialize.
            ActiveRepositories = Array.Empty<ILocalRepositoryModel>();

            // The IGitExt service isn't available when a TFS based solution is opened directly.
            // It will become available when moving to a Git based solution (and cause a UIContext event to fire).
            var context = factory.GetUIContext(new Guid(Guids.GitSccProviderId));
            context.WhenActivated(() => Initialize());
        }

        void Initialize()
        {
            PendingTasks = asyncServiceProvider.GetServiceAsync(typeof(IGitExt)).ContinueWith(t =>
            {
                gitService = (IGitExt)t.Result;
                if (gitService == null)
                {
                    log.Error("Couldn't find IGitExt service");
                    return;
                }

                RefreshActiveRepositories();
                gitService.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(gitService.ActiveRepositories))
                    {
                        RefreshActiveRepositories();
                    }
                };
            }, TaskScheduler.Default);
        }

        public void RefreshActiveRepositories()
        {
            try
            {
                lock (refreshLock)
                {
                    log.Debug(
                        "IGitExt.ActiveRepositories (#{Id}) returned {Repositories}",
                        gitService.GetHashCode(),
                        gitService.ActiveRepositories.Select(x => x.RepositoryPath));

                    ActiveRepositories = gitService?.ActiveRepositories.Select(x => repositoryFactory.Create(x.RepositoryPath)).ToList();
                }
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