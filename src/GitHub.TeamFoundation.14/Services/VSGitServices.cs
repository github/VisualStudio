#if TEAMEXPLORER15
// Microsoft.VisualStudio.Shell.Framework has an alias to avoid conflict with IAsyncServiceProvider
extern alias SF15;
using ServiceProgressData = SF15::Microsoft.VisualStudio.Shell.ServiceProgressData;
#endif

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.TeamFoundation;
using GitHub.VisualStudio;
#if TEAMEXPLORER14
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using ReactiveUI;
#else
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading;
#endif
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using Serilog;

namespace GitHub.Services
{
    [Export(typeof(IVSGitServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSGitServices : IVSGitServices
    {
        static readonly ILogger log = LogManager.ForContext<VSGitServices>();
        readonly IGitHubServiceProvider serviceProvider;
#if TEAMEXPLORER15
        readonly Lazy<IStatusBarNotificationService> statusBar;
#endif

        /// <summary>
        /// This MEF export requires specific versions of TeamFoundation. IGitExt is declared here so
        /// that instances of this type cannot be created if the TeamFoundation dlls are not available
        /// (otherwise we'll have multiple instances of IVSServices exports, and that would be Bad(tm))
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        IGitExt gitExtService;

        [ImportingConstructor]
        public VSGitServices(IGitHubServiceProvider serviceProvider, Lazy<IStatusBarNotificationService> statusBar)
        {
            this.serviceProvider = serviceProvider;
#if TEAMEXPLORER15
            this.statusBar = statusBar;
#endif
        }

        // The Default Repository Path that VS uses is hidden in an internal
        // service 'ISccSettingsService' registered in an internal service
        // 'ISccServiceHost' in an assembly with no public types that's
        // always loaded with VS if the git service provider is loaded
        public string GetLocalClonePathFromGitProvider()
        {
            string ret = string.Empty;

            try
            {
                ret = RegistryHelper.PokeTheRegistryForLocalClonePath();
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error loading the default cloning path from the registry");
            }
            return ret;
        }

        /// <inheritdoc/>
        public async Task Clone(
            string cloneUrl,
            string clonePath,
            bool recurseSubmodules,
            object progress = null)
        {
#if TEAMEXPLORER14
            var gitExt = serviceProvider.GetService<IGitRepositoriesExt>();
            gitExt.Clone(cloneUrl, clonePath, recurseSubmodules ? CloneOptions.RecurseSubmodule : CloneOptions.None);

            // The operation will have completed when CanClone goes false and then true again.
            await gitExt.WhenAnyValue(x => x.CanClone).Where(x => !x).Take(1);
            await gitExt.WhenAnyValue(x => x.CanClone).Where(x => x).Take(1);
#else
            var gitExt = serviceProvider.GetService<IGitActionsExt>();
            var typedProgress = ((Progress<ServiceProgressData>)progress) ?? new Progress<ServiceProgressData>();

            await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                typedProgress.ProgressChanged += (s, e) => statusBar.Value.ShowMessage(e.ProgressText);
                await gitExt.CloneAsync(cloneUrl, clonePath, recurseSubmodules, default(CancellationToken), typedProgress);
            });
#endif
        }

        IGitRepositoryInfo GetRepoFromVS()
        {
            gitExtService = serviceProvider.GetService<IGitExt>();
            return gitExtService.ActiveRepositories.FirstOrDefault();
        }

        public LibGit2Sharp.IRepository GetActiveRepo()
        {
            var repo = GetRepoFromVS();
            return repo != null
                ? serviceProvider.GetService<IGitService>().GetRepository(repo.RepositoryPath)
                : serviceProvider.GetSolution().GetRepositoryFromSolution();
        }

        public string GetActiveRepoPath()
        {
            string ret = null;
            var repo = GetRepoFromVS();
            if (repo != null)
                ret = repo.RepositoryPath;
            if (ret == null)
            {
                using (var repository = serviceProvider.GetSolution().GetRepositoryFromSolution())
                {
                    ret = repository?.Info?.Path;
                }
            }
            return ret ?? String.Empty;
        }

        public IEnumerable<ILocalRepositoryModel> GetKnownRepositories()
        {
            try
            {
                return RegistryHelper.PokeTheRegistryForRepositoryList();
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error loading the repository list from the registry");
                return Enumerable.Empty<ILocalRepositoryModel>();
            }
        }

        public string SetDefaultProjectPath(string path)
        {
            return RegistryHelper.SetDefaultProjectPath(path);
        }
    }
}
