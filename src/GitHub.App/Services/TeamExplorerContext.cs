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
    /// This service uses reflection to access the IGitExt from Visual Studio 2015 and 2017.
    /// </summary>
    [Export(typeof(ITeamExplorerContext))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerContext : ITeamExplorerContext
    {
        const string GitExtTypeName = "Microsoft.VisualStudio.TeamFoundation.Git.Extensibility.IGitExt, Microsoft.TeamFoundation.Git.Provider";

        readonly ILogger log;
        readonly DTE dte;

        string solutionPath;
        string repositoryPath;
        string branchName;
        string headSha;
        string trackedSha;

        readonly IService service = new Service();

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

            dte = serviceProvider.TryGetService<DTE>();
            if (dte == null)
            {
                log.Error("Couldn't find service for type {DteType}", typeof(DTE));
            }

            Refresh(gitExt);

            var notifyPropertyChanged = gitExt as INotifyPropertyChanged;
            if (notifyPropertyChanged == null)
            {
                log.Error("The service {ServiceObject} doesn't implement {Interface}", gitExt, typeof(INotifyPropertyChanged));
                return;
            }

            notifyPropertyChanged.PropertyChanged += (s, e) => Refresh(gitExt);
        }

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

        void Refresh(object gitExt)
        {
            try
            {
                var repo = FindActiveRepository(gitExt);
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
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveRepository)));
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

        static RepositoryInfo FindActiveRepository(object gitExt)
        {
            var activeRepositoriesProperty = gitExt.GetType().GetProperty("ActiveRepositories");
            var activeRepositories = (IEnumerable<object>)activeRepositoriesProperty?.GetValue(gitExt);
            var repo = activeRepositories?.FirstOrDefault();
            if (repo == null)
            {
                return null;
            }

            var repositoryPathProperty = repo.GetType().GetProperty("RepositoryPath");
            var repositoryPath = (string)repositoryPathProperty?.GetValue(repo);

            var currentBranchProperty = repo.GetType().GetProperty("CurrentBranch");
            var currentBranch = currentBranchProperty?.GetValue(repo);

            var headShaProperty = currentBranch?.GetType().GetProperty("HeadSha");
            var headSha = (string)headShaProperty?.GetValue(currentBranch);

            var nameProperty = currentBranch?.GetType().GetProperty("Name");
            var branchName = (string)nameProperty?.GetValue(currentBranch);

            return new RepositoryInfo(repositoryPath, branchName, headSha);
        }

        class RepositoryInfo
        {
            public RepositoryInfo(string repositoryPath, string branchName, string headSha)
            {
                RepositoryPath = repositoryPath;
                BranchName = branchName;
                HeadSha = headSha;
            }

            public string RepositoryPath { get; }
            public string BranchName { get; }
            public string HeadSha { get; }
        }

        public ILocalRepositoryModel ActiveRepository
        {
            get; private set;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler StatusChanged;
    }
}
