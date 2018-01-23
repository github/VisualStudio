using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using GitHub.Models;
using GitHub.Logging;
using Serilog;
using EnvDTE;
using LibGit2Sharp;

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
        const string GitExtTypeName = "Microsoft.VisualStudio.TeamFoundation.Git.Extensibility.IGitExt, Microsoft.TeamFoundation.Git.Provider";

        readonly ILogger log;
        readonly DTE dte;
        readonly IService service;
        readonly GitExt gitExt;

        string solutionPath;
        string repositoryPath;
        string branchName;
        string headSha;
        string trackedSha;

        ILocalRepositoryModel repositoryModel;


        [ImportingConstructor]
        public TeamExplorerContext(IGitHubServiceProvider serviceProvider)
            : this(LogManager.ForContext<TeamExplorerContext>(), new Service(), GitExtTypeName, serviceProvider)
        {
        }

        public TeamExplorerContext(ILogger log, IService service, string gitExtTypeName, IGitHubServiceProvider serviceProvider)
        {
            this.log = log;
            this.service = service;

            // Visual Studio 2015 and 2017 use different versions of the Microsoft.TeamFoundation.Git.Provider assembly.
            // There are no binding redirections between them, but the package that includes them defines a ProvideBindingPath
            // attrubute. This means the required IGitExt type can be found using an unqualified assembly name (GitExtTypeName).
            var gitExtType = Type.GetType(gitExtTypeName, false);
            if (gitExtType == null)
            {
                log.Error("Couldn't find type {GitExtTypeName}", gitExtTypeName);
            }

            var gitExt = serviceProvider.GetService(gitExtType);
            if (gitExt == null)
            {
                log.Error("Couldn't find service for type {GitExtType}", gitExtType);
                return;
            }

            this.gitExt = new GitExt(gitExt);

            dte = serviceProvider.TryGetService<DTE>();
            if (dte == null)
            {
                log.Error("Couldn't find service for type {DteType}", typeof(DTE));
            }

            Refresh();

            var notifyPropertyChanged = gitExt as INotifyPropertyChanged;
            if (notifyPropertyChanged == null)
            {
                log.Error("The service {ServiceObject} doesn't implement {Interface}", gitExt, typeof(INotifyPropertyChanged));
                return;
            }

            notifyPropertyChanged.PropertyChanged += (s, e) => Refresh();
        }

        /// <summary>
        /// Used for unit testing.
        /// </summary>
        public interface IService
        {
            string FindTrackedSha(string repositoryPath);
            ILocalRepositoryModel CreateRepository(string path);
        }

        class Service : IService
        {
            public string FindTrackedSha(string repositoryPath)
            {
                using (var repo = new Repository(repositoryPath))
                {
                    return repo.Head.TrackedBranch?.Tip.Sha;
                }
            }

            public ILocalRepositoryModel CreateRepository(string path) => new LocalRepositoryModel(path);
        }

        void Refresh()
        {
            try
            {
                var repo = gitExt.ActiveRepository;
                var newSolutionPath = dte?.Solution?.FullName;

                if (repo == null && newSolutionPath == solutionPath)
                {
                    // Ignore when ActiveRepositories is empty and solution hasn't changed.
                    // https://github.com/github/VisualStudio/issues/1421
                    log.Information("Ignoring no ActiveRepository when solution hasn't changed");
                }
                else
                {
                    var newRepositoryPath = repo?.RepositoryPath;
                    var newBranchName = repo?.BranchName;
                    var newHeadSha = repo?.HeadSha;
                    var newTrackedSha = newRepositoryPath != null ? service.FindTrackedSha(newRepositoryPath) : null;

                    if (newRepositoryPath != repositoryPath)
                    {
                        log.Information("Fire PropertyChanged event for ActiveRepository");
                        ActiveRepository = repo != null ? service.CreateRepository(repo.RepositoryPath) : null;
                    }
                    else if (newBranchName != branchName)
                    {
                        log.Information("Fire StatusChanged event when BranchName changes for ActiveRepository");
                        StatusChanged?.Invoke(this, EventArgs.Empty);
                    }
                    else if (newHeadSha != headSha)
                    {
                        log.Information("Fire StatusChanged event when HeadSha changes for ActiveRepository");
                        StatusChanged?.Invoke(this, EventArgs.Empty);
                    }
                    else if (newTrackedSha != trackedSha)
                    {
                        log.Information("Fire StatusChanged event when TrackedSha changes for ActiveRepository");
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

        class GitExt
        {
            readonly object gitExt;

            internal GitExt(object gitExt)
            {
                this.gitExt = gitExt;
            }

            internal RepositoryInfo ActiveRepository
            {
                get
                {
                    var activeRepositories = (IEnumerable<object>)gitExt.GetType().GetProperty("ActiveRepositories")?.GetValue(gitExt);
                    var repo = activeRepositories?.FirstOrDefault();
                    return repo != null ? new RepositoryInfo(repo) : null;
                }
            }
        }

        class RepositoryInfo
        {
            internal RepositoryInfo(object repo)
            {
                // Only called a handful times per session so not caching reflection calls.
                var currentBranch = repo.GetType().GetProperty("CurrentBranch")?.GetValue(repo);
                RepositoryPath = (string)repo.GetType().GetProperty("RepositoryPath")?.GetValue(repo);
                BranchName = (string)currentBranch?.GetType().GetProperty("Name")?.GetValue(currentBranch);
                HeadSha = (string)currentBranch?.GetType().GetProperty("HeadSha")?.GetValue(currentBranch);
            }

            public string RepositoryPath { get; }
            public string BranchName { get; }
            public string HeadSha { get; }
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
