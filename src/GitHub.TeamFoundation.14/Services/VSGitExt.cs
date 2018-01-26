using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using GitHub.Models;
using GitHub.Services;
using GitHub.Logging;
using Serilog;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;

namespace GitHub.VisualStudio.Base
{
    [Export(typeof(IVSGitExt))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSGitExt : IVSGitExt
    {
        static readonly ILogger log = LogManager.ForContext<VSGitExt>();

        IGitHubServiceProvider serviceProvider;
        IVSUIContext context;
        IGitExt gitService;

        [ImportingConstructor]
        public VSGitExt(IGitHubServiceProvider serviceProvider, IVSUIContextFactory factory)
        {
            this.serviceProvider = serviceProvider;

            // The IGitExt service is only available when in the SccProvider context.
            // This could be changed to VSConstants.UICONTEXT.SolutionExists_guid when testing.
            context = factory.GetUIContext(new Guid(Guids.GitSccProviderId));

            // If we're not in the GitSccProvider context or TryInitialize fails, have another go when the context changes.
            if (!context.IsActive || !TryInitialize())
            {
                context.UIContextChanged += ContextChanged;
                log.Information("VSGitExt will be initialized later");
            }
        }

        void ContextChanged(object sender, VSUIContextChangedEventArgs e)
        {
            // If we're in the GitSccProvider context and TryInitialize succeeds, we can stop listening for events.
            if (e.Activated && TryInitialize())
            {
                context.UIContextChanged -= ContextChanged;
                log.Information("Initialized VSGitExt on UIContextChanged");
            }
        }

        bool TryInitialize()
        {
            gitService = serviceProvider.GetService<IGitExt>();
            if (gitService != null)
            {
                // The IGitExt service is now available so let consumers know to read ActiveRepositories.
                ActiveRepositoriesChanged?.Invoke();
                gitService.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(gitService.ActiveRepositories))
                    {
                        ActiveRepositoriesChanged?.Invoke();
                    }
                };

                log.Information("Found IGitExt service and initialized VSGitExt");
                return true;
            }

            log.Information("Couldn't find IGitExt service");
            return false;
        }

        public IEnumerable<ILocalRepositoryModel> ActiveRepositories => gitService?.ActiveRepositories.Select(x => x.ToModel());
        public event Action ActiveRepositoriesChanged;
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