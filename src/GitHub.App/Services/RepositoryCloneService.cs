using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Octokit;
using Rothko;

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
        readonly IOperatingSystem operatingSystem;

        [ImportingConstructor]
        public RepositoryCloneService(Lazy<IServiceProvider> serviceProvider, IOperatingSystem operatingSystem)
        {
            this.serviceProvider = serviceProvider;
            this.operatingSystem = operatingSystem;
        }

        private IServiceProvider ServiceProvider
        {
            get { return serviceProvider.Value; }
        }

        public IObservable<Unit> CloneRepository(string cloneUrl, string repositoryName, string repositoryPath)
        {
            Guard.ArgumentNotEmptyString(cloneUrl, "cloneUrl");
            Guard.ArgumentNotEmptyString(repositoryName, "repositoryName");
            Guard.ArgumentNotEmptyString(repositoryPath, "repositoryPath");

            return Observable.Start(() =>
            {
                string path = Path.Combine(repositoryPath, repositoryName);

                operatingSystem.Directory.CreateDirectory(path);

                var gitExt = ServiceProvider.GetService(typeof(IGitRepositoriesExt)) as IGitRepositoriesExt;
                Debug.Assert(gitExt != null, "Could not get an instance of IGitRepositoriesExt");

                gitExt.Clone(cloneUrl, path, CloneOptions.RecurseSubmodule);
                return Unit.Default;
            });
        }
    }
}
