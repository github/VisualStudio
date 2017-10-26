using System;
using System.ComponentModel.Composition;
using System.IO;
using GitHub.Extensions;
using NLog;
using Rothko;
using GitHub.Helpers;
using Task = System.Threading.Tasks.Task;

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
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly IOperatingSystem operatingSystem;
        readonly string defaultClonePath;
        readonly IVSGitServices vsGitServices;
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        public RepositoryCloneService(
            IOperatingSystem operatingSystem,
            IVSGitServices vsGitServices,
            IUsageTracker usageTracker)
        {
            this.operatingSystem = operatingSystem;
            this.vsGitServices = vsGitServices;
            this.usageTracker = usageTracker;

            defaultClonePath = GetLocalClonePathFromGitProvider(operatingSystem.Environment.GetUserRepositoriesPath());
        }

        /// <inheritdoc/>
        public async Task CloneRepository(
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
                await usageTracker.IncrementCloneCount();
            }
            catch (Exception ex)
            {
                log.Error("Could not clone {0} to {1}. {2}", cloneUrl, path, ex);
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
