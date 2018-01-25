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
        public VSGitExt(IGitHubServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            var factory = serviceProvider.GetService<IVSUIContextFactory>();

            // The IGitExt service is only available when in the SccProvider context.
            // This could be changed to VSConstants.UICONTEXT.SolutionExists_guid when testing.
            context = factory.GetUIContext(new Guid(Guids.GitSccProviderId));

            // If we're not in the GitSccProvider context or TryInitialize fails, have another go when the context changes.
            if (!context.IsActive || !TryInitialize())
            {
                context.UIContextChanged += ContextChanged;
            }
        }

        void ContextChanged(object sender, IVSUIContextChangedEventArgs e)
        {
            // If we're in the GitSccProvider context and TryInitialize succeeds, we can stop listening for events.
            if (e.Activated && TryInitialize())
            {
                context.UIContextChanged -= ContextChanged;
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

                return true;
            }

            return false;
        }

        public IEnumerable<ILocalRepositoryModel> ActiveRepositories => gitService?.ActiveRepositories.Select(x => x.ToModel());
        public event Action ActiveRepositoriesChanged;
    }
}