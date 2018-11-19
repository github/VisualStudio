using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.TeamFoundation;
using GitHub.VisualStudio;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using ReactiveUI;
using Serilog;
using Microsoft;

namespace GitHub.Services
{
    [Export(typeof(IVSGitServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSGitServices : IVSGitServices
    {
        static readonly ILogger log = LogManager.ForContext<VSGitServices>();

        readonly IGitHubServiceProvider serviceProvider;

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used in VS2017")]
        readonly Lazy<IStatusBarNotificationService> statusBar;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used in VS2015")]
        readonly Lazy<ITeamExplorerServices> teamExplorerServices;

        /// <summary>
        /// This MEF export requires specific versions of TeamFoundation. IGitExt is declared here so
        /// that instances of this type cannot be created if the TeamFoundation dlls are not available
        /// (otherwise we'll have multiple instances of IVSServices exports, and that would be Bad(tm))
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        IGitExt gitExtService;

        [ImportingConstructor]
        public VSGitServices(IGitHubServiceProvider serviceProvider,
            Lazy<IStatusBarNotificationService> statusBar,
            Lazy<ITeamExplorerServices> teamExplorerServices)
        {
            this.serviceProvider = serviceProvider;
            this.statusBar = statusBar;
            this.teamExplorerServices = teamExplorerServices;
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
            var teamExplorer = serviceProvider.TryGetService<ITeamExplorer>();
            Assumes.Present(teamExplorer);

#if TEAMEXPLORER14
            await StartClonenOnConnectPageAsync(teamExplorer, cloneUrl, clonePath, recurseSubmodules);
            NavigateToHomePage(teamExplorer); // Show progress on Team Explorer - Home
            await WaitForCloneOnHomePageAsync(teamExplorer);
#elif TEAMEXPLORER15 || TEAMEXPLORER16
            // The ServiceProgressData type is in a Visual Studio 2019 assembly that we don't currently have access to.
            // Using reflection to call the CloneAsync in order to avoid conflicts with the Visual Studio 2017 version.
            // Progress won't be displayed on the status bar, but it appears prominently on the Team Explorer Home view.
            var gitExt = serviceProvider.GetService<IGitActionsExt>();
            var cloneAsyncMethod = typeof(IGitActionsExt).GetMethod(nameof(IGitActionsExt.CloneAsync));
            Assumes.NotNull(cloneAsyncMethod);
            var cloneParameters = new object[] { cloneUrl, clonePath, recurseSubmodules, null, null };
            var cloneTask = (Task)cloneAsyncMethod.Invoke(gitExt, cloneParameters);

            NavigateToHomePage(teamExplorer); // Show progress on Team Explorer - Home
            await cloneTask;
#endif
            // Change Team Explorer context to the newly cloned repository
            teamExplorerServices.Value.OpenRepository(clonePath);
        }

        static async Task StartClonenOnConnectPageAsync(
            ITeamExplorer teamExplorer, string cloneUrl, string clonePath, bool recurseSubmodules)
        {
            var connectPage = await NavigateToPageAsync(teamExplorer, new Guid(TeamExplorerPageIds.Connect));
            Assumes.Present(connectPage);
            var gitExt = connectPage.GetService<IGitRepositoriesExt>();
            Assumes.Present(gitExt);

            gitExt.Clone(cloneUrl, clonePath, recurseSubmodules ? CloneOptions.RecurseSubmodule : CloneOptions.None);
        }

        static async Task WaitForCloneOnHomePageAsync(ITeamExplorer teamExplorer)
        {
            // The clone progress bar appears on the GettingStartedSection of the Home page,
            // so we wait for this to be hidden before continuing.
            var sectionId = new Guid("d0200918-c025-4cc3-9dee-4f5e89d0c918"); // GettingStartedSection
            await teamExplorer
                .WhenAnyValue(x => x.CurrentPage)
                .Where(p => p.GetId() == new Guid(TeamExplorerPageIds.Home))
                .Select(p => p.GetSection(sectionId))
                .Where(s => s != null)
                .Select(s => s.WhenAnyValue(x => x.IsVisible))
                .Switch()                           // Watch the topmost section
                .StartWith(false)                   // If no events arrive default to invisible
                .Throttle(TimeSpan.FromSeconds(1))  // Ignore glitch where section starts invisible
                .Any(x => x == false);
        }

        static void NavigateToHomePage(ITeamExplorer teamExplorer)
        {
            teamExplorer.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null);
        }

        static async Task<ITeamExplorerPage> NavigateToPageAsync(ITeamExplorer teamExplorer, Guid pageId)
        {
            teamExplorer.NavigateToPage(pageId, null);
            var page = await teamExplorer
                .WhenAnyValue(x => x.CurrentPage)
                .Where(x => x?.GetId() == pageId)
                .Take(1);
            return page;
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

        public IEnumerable<LocalRepositoryModel> GetKnownRepositories()
        {
            try
            {
                return RegistryHelper.PokeTheRegistryForRepositoryList();
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error loading the repository list from the registry");
                return Enumerable.Empty<LocalRepositoryModel>();
            }
        }

        public string SetDefaultProjectPath(string path)
        {
            return RegistryHelper.SetDefaultProjectPath(path);
        }
    }
}
