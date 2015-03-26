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

                var gitExt = ServiceProvider.TryGetService<IGitRepositoriesExt>();
                Debug.Assert(gitExt != null, "Could not get an instance of IGitRepositoriesExt");

                gitExt.Clone(cloneUrl, path, CloneOptions.RecurseSubmodule);
                return Unit.Default;
            });
        }

        // The Default Repository Path that VS uses is hidden in an internal
        // service 'ISccSettingsService' registered in an internal service
        // 'ISccServiceHost' in an assembly with no public types that's
        // always loaded with VS if the git service provider is loaded
        public string GetLocalClonePathFromGitProvider(string fallbackPath)
        {
            var provider = (IUIProvider)ServiceProvider;
            var ret = fallbackPath;
            try
            {
                var ns = "Microsoft.TeamFoundation.Git.CoreServices.";
                var scchost = "ISccServiceHost";
                var sccservice = "ISccSettingsService";

                var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Microsoft.TeamFoundation.Git.CoreServices");
                if (asm == null)
                    return ret;

                var type = asm.GetType(ns + scchost);
                if (type == null)
                    return ret;
                var hostService = provider.TryGetService(type) as IServiceProvider;
                if (hostService == null)
                    return ret;
                type = asm.GetType(ns + sccservice);
                if (type == null)
                    return ret;
                var settings = hostService.GetService(type);
                if (settings == null)
                    return ret;
                var prop = type.GetProperty("DefaultRepositoryPath");
                if (prop == null)
                    return ret;
                var getm = prop.GetGetMethod();
                ret = (string)getm.Invoke(settings, null);
                ret = System.Environment.ExpandEnvironmentVariables(ret);
            }
            catch { }

            return ret;
        }

        public string DefaultClonePath { get { return defaultClonePath; } }

        IServiceProvider ServiceProvider { get { return serviceProvider.Value; } }

    }
}
