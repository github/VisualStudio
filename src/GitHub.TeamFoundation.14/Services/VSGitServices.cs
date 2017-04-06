using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.TeamFoundation;
using GitHub.VisualStudio;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using ReactiveUI;
using Rothko;
using EnvDTE;

namespace GitHub.Services
{
    [NullGuard.NullGuard(NullGuard.ValidationFlags.None)]
    [Export(typeof(IVSGitServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSGitServices : IVSGitServices
    {
        // Use a prefix (~$) that is defined in the default VS gitignore.
        public const string TempSolutionName = "~$GitHubVSTemp$~";

        readonly IGitHubServiceProvider serviceProvider;
        readonly IVsStatusbar statusBar;
        readonly IVSServices vsServices;
        readonly IOperatingSystem operatingSystem;

        /// <summary>
        /// This MEF export requires specific versions of TeamFoundation. IGitExt is declared here so
        /// that instances of this type cannot be created if the TeamFoundation dlls are not available
        /// (otherwise we'll have multiple instances of IVSServices exports, and that would be Bad(tm))
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        IGitExt gitExtService;

        [ImportingConstructor]
        public VSGitServices(IGitHubServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            statusBar = serviceProvider.GetService<IVsStatusbar>();
            vsServices = serviceProvider.TryGetService<IVSServices>();
            operatingSystem = serviceProvider.TryGetService<IOperatingSystem>();
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
                VsOutputLogger.WriteLine(string.Format(CultureInfo.CurrentCulture, "Error loading the default cloning path from the registry '{0}'", ex));
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
            var typedProgress = ((Progress<Microsoft.VisualStudio.Shell.ServiceProgressData>)progress) ??
                new Progress<Microsoft.VisualStudio.Shell.ServiceProgressData>();

            await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                typedProgress.ProgressChanged += (s, e) => statusBar.SetText(e.ProgressText);
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
                ret = serviceProvider.GetSolution().GetRepositoryFromSolution()?.Info?.Path;
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
                VsOutputLogger.WriteLine(string.Format(CultureInfo.CurrentCulture, "Error loading the repository list from the registry '{0}'", ex));
                return Enumerable.Empty<ILocalRepositoryModel>();
            }
        }

        public string SetDefaultProjectPath(string path)
        {
            return RegistryHelper.SetDefaultProjectPath(path);
        }

        /// <summary>Open a repository in Team Explorer</summary>
        /// <remarks>
        /// There doesn't appear to be a command that directly opens a target repo.
        /// Our workaround is to create, open and delete a solution in the repo directory.
        /// This triggers an event that causes the target repo to open. ;)
        /// </remarks>
        /// <param name="repoPath">The path to the repository to open</param>
        /// <returns>True if a transient solution was successfully created in target directory (which should trigger opening of repository).</returns>
        public bool TryOpenRepository(string repoPath)
        {
            var os = serviceProvider.TryGetService<IOperatingSystem>();
            if (os == null)
            {
                VsOutputLogger.WriteLine("TryOpenRepository couldn't find IOperatingSystem service.");
                return false;
            }

            var dte = serviceProvider.TryGetService<DTE>();
            if (dte == null)
            {
                VsOutputLogger.WriteLine("TryOpenRepository couldn't find DTE service.");
                return false;
            }

            var repoDir = os.Directory.GetDirectory(repoPath);
            if (!repoDir.Exists)
            {
                return false;
            }

            bool solutionCreated = false;
            try
            {
                dte.Solution.Create(repoPath, TempSolutionName);
                solutionCreated = true;

                dte.Solution.Close(false); // Don't create a .sln file when we close.
            }
            catch (Exception e)
            {
                VsOutputLogger.WriteLine("Error opening repository. {0}", e);
            }
            finally
            {
                TryCleanupSolutionUserFiles(os, repoPath, TempSolutionName);
            }
            return solutionCreated;
        }

        void TryCleanupSolutionUserFiles(IOperatingSystem os, string repoPath, string slnName)
        {
            var vsTempPath = Path.Combine(repoPath, ".vs", slnName);
            try
            {
                // Clean up the dummy solution's subdirectory inside `.vs`.
                var vsTempDir = os.Directory.GetDirectory(vsTempPath);
                if (vsTempDir.Exists)
                {
                    vsTempDir.Delete(true);
                }
            }
            catch (Exception e)
            {
                VsOutputLogger.WriteLine("Couldn't clean up {0}. {1}", vsTempPath, e);
            }
        }

    }
}
