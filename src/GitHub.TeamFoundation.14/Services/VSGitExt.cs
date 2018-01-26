using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using GitHub.Models;
using GitHub.Services;
using GitHub.Logging;
using Serilog;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio.Base
{
    [Export(typeof(IVSGitExt))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSGitExt : IVSGitExt
    {
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

    [Export(typeof(IVSUIContextFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class VSUIContextFactory : IVSUIContextFactory
    {
        public IVSUIContext GetUIContext(Guid contextGuid)
        {
            return new VSUIContext(UIContext.FromUIContextGuid(contextGuid));
        }
    }

    class VSUIContextChangedEventArgs : IVSUIContextChangedEventArgs
    {
        public bool Activated { get; }

        public VSUIContextChangedEventArgs(bool activated)
        {
            Activated = activated;
        }
    }

    class VSUIContext : IVSUIContext
    {
        readonly UIContext context;
        readonly Dictionary<EventHandler<IVSUIContextChangedEventArgs>, EventHandler<UIContextChangedEventArgs>> handlers =
            new Dictionary<EventHandler<IVSUIContextChangedEventArgs>, EventHandler<UIContextChangedEventArgs>>();
        public VSUIContext(UIContext context)
        {
            this.context = context;
        }

        public bool IsActive { get { return context.IsActive; } }

        public event EventHandler<IVSUIContextChangedEventArgs> UIContextChanged
        {
            add
            {
                EventHandler<UIContextChangedEventArgs> handler = null;
                if (!handlers.TryGetValue(value, out handler))
                {
                    handler = (s, e) => value.Invoke(s, new VSUIContextChangedEventArgs(e.Activated));
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