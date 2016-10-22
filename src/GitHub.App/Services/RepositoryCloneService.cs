using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Extensions;
using GitHub.Infrastructure;
using Microsoft.VisualStudio.Shell;
using Serilog;
using Rothko;
using GitHub.Helpers;

namespace GitHub.Services
{
    /// <summary>
    /// Service used to clone GitHub repositories. It wraps the
    /// <see cref="Microsoft.TeamFoundation.Git.Controls.Extensibility.IGitRepositoriesExt"/> service provided
    /// by Team Explorer.
    /// </summary>
    [Export(typeof(IRepositoryCloneService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryCloneService : IRepositoryCloneService
    {
        static readonly ILogger log = LogManager.ForContext<RepositoryCloneService>();

        readonly IOperatingSystem operatingSystem;
        readonly string defaultClonePath;
        readonly IVSGitServices vsGitServices;

        [ImportingConstructor]
        public RepositoryCloneService(IOperatingSystem operatingSystem, IVSGitServices vsGitServices)
        {
            this.operatingSystem = operatingSystem;
            this.vsGitServices = vsGitServices;

            defaultClonePath = GetLocalClonePathFromGitProvider(operatingSystem.Environment.GetUserRepositoriesPath());
        }

        public IObservable<Unit> CloneRepository(string cloneUrl, string repositoryName, string repositoryPath)
        {
            Guard.ArgumentNotEmptyString(cloneUrl, nameof(cloneUrl));
            Guard.ArgumentNotEmptyString(repositoryName, nameof(repositoryName));
            Guard.ArgumentNotEmptyString(repositoryPath, nameof(repositoryPath));

            return Observable.StartAsync(async () =>
            {
                string path = Path.Combine(repositoryPath, repositoryName);

                operatingSystem.Directory.CreateDirectory(path);

                // Once we've done IO switch to the main thread to call vsGitServices.Clone() as this must be
                // called on the main thread.
                await ThreadingHelper.SwitchToMainThreadAsync();

                try
                {
                    // this will throw if it can't find it
                    vsGitServices.Clone(cloneUrl, path, true);
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Could not clone {cloneUrl} to {path}", cloneUrl, path);
                    throw;
                }
                
                return Unit.Default;
            });
        }

        string GetLocalClonePathFromGitProvider(string fallbackPath)
        {
            var ret = vsGitServices.GetLocalClonePathFromGitProvider();
            return !string.IsNullOrEmpty(ret)
                ? operatingSystem.Environment.ExpandEnvironmentVariables(ret)
                : fallbackPath;
        }

        public string DefaultClonePath { get { return defaultClonePath; } }
    }
}
