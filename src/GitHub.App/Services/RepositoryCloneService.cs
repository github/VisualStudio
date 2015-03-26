using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Rothko;
using System.Linq;
using GitHub.Extensions;
using GitHub.Info;
using System.Globalization;

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
        readonly string defaultClonePath;


        [ImportingConstructor]
        public RepositoryCloneService(Lazy<IServiceProvider> serviceProvider, IOperatingSystem operatingSystem)
        {
            this.serviceProvider = serviceProvider;
            this.operatingSystem = operatingSystem;

            defaultClonePath = operatingSystem.Environment.GetUserDocumentsPathForApplication();
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

                // this will throw if it can't find it
                VSServices.Clone(ServiceProvider, cloneUrl, path, true);
                return Unit.Default;
            });
        }

        public string GetLocalClonePathFromGitProvider(string fallbackPath)
        {
            var ret = VSServices.GetLocalClonePathFromGitProvider(ServiceProvider);
            if (!string.IsNullOrEmpty(ret))
                ret = operatingSystem.Environment.ExpandEnvironmentVariables(ret);
            else
                ret = fallbackPath;
            return ret;
        }

        public string DefaultClonePath { get { return defaultClonePath; } }

        IServiceProvider ServiceProvider { get { return serviceProvider.Value; } }
        IVSServices VSServices { get { return ServiceProvider.GetService<IVSServices>(); } }
    }
}
