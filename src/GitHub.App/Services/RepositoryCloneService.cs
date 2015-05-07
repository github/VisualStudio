using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using Rothko;
using GitHub.Extensions;

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
        readonly IOperatingSystem operatingSystem;
        readonly string defaultClonePath;
        readonly IVSServices vsservices;


        [ImportingConstructor]
        public RepositoryCloneService(IOperatingSystem operatingSystem, IVSServices vsservices)
        {
            this.operatingSystem = operatingSystem;
            this.vsservices = vsservices;

            defaultClonePath = GetLocalClonePathFromGitProvider(operatingSystem.Environment.GetUserDocumentsPathForApplication());
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
                vsservices.Clone(cloneUrl, path, true);
                return Unit.Default;
            });
        }

        string GetLocalClonePathFromGitProvider(string fallbackPath)
        {
            var ret = vsservices.GetLocalClonePathFromGitProvider();
            if (!string.IsNullOrEmpty(ret))
                ret = operatingSystem.Environment.ExpandEnvironmentVariables(ret);
            else
                ret = fallbackPath;
            return ret;
        }

        public string DefaultClonePath { get { return defaultClonePath; } }
    }
}
