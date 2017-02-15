using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.VisualStudio;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using GitHub.TeamFoundation;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using Microsoft.VisualStudio.Shell;
using System.Threading;

namespace GitHub.Services
{
    [NullGuard.NullGuard(NullGuard.ValidationFlags.None)]
    [Export(typeof(IVSGitServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSGitServices : IVSGitServices
    {
        readonly IGitHubServiceProvider serviceProvider;
        readonly IVsStatusbar statusBar;

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
            this.statusBar = serviceProvider.GetService<IVsStatusbar>();
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

        public void Clone(string cloneUrl, string clonePath, bool recurseSubmodules)
        {
#if TEAMEXPLORER14
            var gitExt = serviceProvider.GetService<IGitRepositoriesExt>();
            gitExt.Clone(cloneUrl, clonePath, recurseSubmodules ? CloneOptions.RecurseSubmodule : CloneOptions.None);
#else
            var gitExt = serviceProvider.GetService<IGitActionsExt>();
            var progress = new Progress<ServiceProgressData>();

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                progress.ProgressChanged += (s, e) => statusBar.SetText(e.ProgressText);
                await gitExt.CloneAsync(cloneUrl, clonePath, recurseSubmodules, default(CancellationToken), progress);
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
    }
}
