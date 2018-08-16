using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Extensions;
using GitHub.Logging;
using Microsoft.VisualStudio.Shell;
using Serilog;
using Rothko;
using GitHub.Helpers;
using System.Threading.Tasks;
using GitHub.Models;

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
        readonly IGitService gitService;
        readonly IVSGitServices vsGitServices;
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        public RepositoryCloneService(
            IOperatingSystem operatingSystem,
            IGitService gitService,
            IVSGitServices vsGitServices,
            IUsageTracker usageTracker)
        {
            this.operatingSystem = operatingSystem;
            this.gitService = gitService;
            this.vsGitServices = vsGitServices;
            this.usageTracker = usageTracker;

            defaultClonePath = GetLocalClonePathFromGitProvider(operatingSystem.Environment.GetUserRepositoriesPath());
        }

        /// <inheritdoc/>
        public async Task<ILocalRepositoryModel> CloneRepository(
            string cloneUrl,
            string repositoryName,
            string repositoryPath,
            object progress = null)
        {
            Guard.ArgumentNotEmptyString(cloneUrl, nameof(cloneUrl));
            Guard.ArgumentNotEmptyString(repositoryName, nameof(repositoryName));
            Guard.ArgumentNotEmptyString(repositoryPath, nameof(repositoryPath));

            string path = Path.Combine(repositoryPath, repositoryName);

            // Switch to a thread pool thread for IO then back to the main thread to call
            // vsGitServices.Clone() as this must be called on the main thread.
            await ThreadingHelper.SwitchToPoolThreadAsync();
            operatingSystem.Directory.CreateDirectory(path);
            await ThreadingHelper.SwitchToMainThreadAsync();

            try
            {
                await vsGitServices.Clone(cloneUrl, path, true, progress);
                await usageTracker.IncrementCounter(x => x.NumberOfClones);
                return new LocalRepositoryModel(path, gitService);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Could not clone {CloneUrl} to {Path}", cloneUrl, path);
                throw;
            }
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
