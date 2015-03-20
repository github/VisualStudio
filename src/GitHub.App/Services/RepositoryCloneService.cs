using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Octokit;

namespace GitHub.Services
{
    /// <summary>
    /// Service used to clone GitHub repositories. It wraps the <see cref="IGitRepositoriesExt"/> service provided
    /// by Team Explorer.
    /// </summary>
    [Export(typeof(IRepositoryCloneService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RepositoryCloneService : IRepositoryCloneService
    {
        readonly Lazy<IServiceProvider> serviceProvider;

        [ImportingConstructor]
        public RepositoryCloneService(Lazy<IServiceProvider> serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private IServiceProvider ServiceProvider
        {
            get { return serviceProvider.Value; }
        }

        public IObservable<Unit> CloneRepository(string cloneUrl, string repositoryName, string repositoryPath)
        {
            return Observable.Start(() =>
            {
                string path = Path.Combine(repositoryPath, repositoryName);

                var gitExt = ServiceProvider.GetService(typeof(IGitRepositoriesExt)) as IGitRepositoriesExt;
                Debug.Assert(gitExt != null, "Could not get an instance of IGitRepositoriesExt");

                gitExt.Clone(cloneUrl, path, CloneOptions.RecurseSubmodule);
                return Unit.Default;
            });
        }
    }
}
