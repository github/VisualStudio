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
    [Export(typeof(ICloneService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CloneService : ICloneService
    {
        readonly Lazy<IServiceProvider> serviceProvider;

        [ImportingConstructor]
        public CloneService(Lazy<IServiceProvider> serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private IServiceProvider ServiceProvider
        {
            get { return serviceProvider.Value; }
        }

        public IObservable<Unit> CloneRepository(Repository repository, string repositoryPath)
        {
            return Observable.Start(() =>
            {
                string path = Path.Combine(repositoryPath, repository.Name);

                var gitExt = ServiceProvider.GetService(typeof(IGitRepositoriesExt)) as IGitRepositoriesExt;
                Debug.Assert(gitExt != null, "Could not get an instance of IGitRepositoriesExt");

                gitExt.Clone(repository.CloneUrl, path, CloneOptions.RecurseSubmodule);
                return Unit.Default;
            });
        }
    }
}
