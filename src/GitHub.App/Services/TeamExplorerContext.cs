using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Logging;
using Serilog;
using EnvDTE;
using Microsoft.VisualStudio.Threading;

namespace GitHub.Services
{
    /// <summary>
    /// This implementation listenes to IGitExt for ActiveRepositories property change events and fires
    /// <see cref="PropertyChanged"/> and <see cref="StatusChanged"/> events when appropriate.
    /// </summary>
    /// <remarks>
    /// A <see cref="PropertyChanged"/> is fired when a solution is opened in a new repository (or not in a repository).
    /// A <see cref="StatusChanged"/> event is only fired when the current branch, head SHA or tracked SHA changes (not
    /// on every IGitExt property change event). <see cref="ActiveRepository"/> contains the active repository or null
    /// if a solution is opened that isn't in a repository. No events are fired when the same solution is unloaded then
    /// reloaded (e.g. when a .sln file is touched).
    /// </remarks>
    [Export(typeof(ITeamExplorerContext))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerContext : ITeamExplorerContext
    {
        static ILogger log = LogManager.ForContext<TeamExplorerContext>();

        //readonly AsyncLazy<DTE> dte;
        readonly DTE dte;
        readonly IVSGitExt gitExt;
        //readonly IGitHubServiceProvider serviceProvider;

        string solutionPath;
        string repositoryPath;
        string branchName;
        string headSha;
        string trackedSha;

        ILocalRepositoryModel repositoryModel;

        [ImportingConstructor]
        public TeamExplorerContext(IVSGitExt gitExt, DTE dte)
        {
            this.gitExt = gitExt;
            this.dte = dte;
            Refresh();
            gitExt.ActiveRepositoriesChanged += Refresh;
        }

        void Refresh()
        {
            try
            {
                var repo = gitExt.ActiveRepositories?.FirstOrDefault();
                var newSolutionPath = dte.Solution?.FullName;

                if (repo == null && newSolutionPath == solutionPath)
                {
                    // Ignore when ActiveRepositories is empty and solution hasn't changed.
                    // https://github.com/github/VisualStudio/issues/1421
                    log.Debug("Ignoring no ActiveRepository when solution hasn't changed");
                }
                else
                {
                    var newRepositoryPath = repo?.LocalPath;
                    var newBranchName = repo?.CurrentBranch?.Name;
                    var newHeadSha = repo?.CurrentBranch?.Sha;
                    var newTrackedSha = repo?.CurrentBranch?.TrackedSha;

                    if (newRepositoryPath != repositoryPath)
                    {
                        log.Debug("ActiveRepository changed to {CloneUrl} @ {Path}", repo?.CloneUrl, newRepositoryPath);
                        ActiveRepository = repo;
                    }
                    else if (newBranchName != branchName)
                    {
                        log.Debug("Fire StatusChanged event when BranchName changes for ActiveRepository");
                        StatusChanged?.Invoke(this, EventArgs.Empty);
                    }
                    else if (newHeadSha != headSha)
                    {
                        log.Debug("Fire StatusChanged event when HeadSha changes for ActiveRepository");
                        StatusChanged?.Invoke(this, EventArgs.Empty);
                    }
                    else if (newTrackedSha != trackedSha)
                    {
                        log.Debug("Fire StatusChanged event when TrackedSha changes for ActiveRepository");
                        StatusChanged?.Invoke(this, EventArgs.Empty);
                    }

                    repositoryPath = newRepositoryPath;
                    branchName = newBranchName;
                    headSha = newHeadSha;
                    solutionPath = newSolutionPath;
                    trackedSha = newTrackedSha;
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Refreshing active repository");
            }
        }

        /// <summary>
        /// The active repository or null if not in a repository.
        /// </summary>
        public ILocalRepositoryModel ActiveRepository
        {
            get
            {
                return repositoryModel;
            }

            private set
            {
                if (value != repositoryModel)
                {
                    repositoryModel = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveRepository)));
                }
            }
        }

        /// <summary>
        /// Fired when a solution is opened in a new repository (or that isn't in a repository).
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fired when the current branch, head SHA or tracked SHA changes.
        /// </summary>
        public event EventHandler StatusChanged;
    }
}
