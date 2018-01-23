using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using GitHub.Models;
using GitHub.Logging;
using GitHub.Primitives;
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

        [ImportingConstructor]
        public TeamExplorerContext(IGitHubServiceProvider serviceProvider)
            : this(serviceProvider, LogManager.ForContext<TeamExplorerContext>(), null)
        {
        }

        public TeamExplorerContext(IGitHubServiceProvider serviceProvider, ILogger log, Type gitExtType)
        {
            this.log = log;

            gitExtType = gitExtType ?? FindGitExtType();
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

        Type FindGitExtType()
        {
            var gitExtType = Type.GetType(GitExtTypeName, false);
            if (gitExtType == null)
            {
                log.Error("Couldn't find type {GitExtTypeName}", GitExtTypeName);
            }

            return gitExtType;
        }

        void Refresh(object gitExt)
        {
            try
            {
                string newRepositoryPath;
                string newBranchName;
                string newHeadSha;
                FindActiveRepository(gitExt, out newRepositoryPath, out newBranchName, out newHeadSha);
                var newSolutionPath = dte?.Solution?.FullName;
                var newTrackedSha = newRepositoryPath != null ? FindTrackedSha(newRepositoryPath) : null;

                if (newRepositoryPath == null && newSolutionPath == solutionPath)
                {
                    // Ignore when ActiveRepositories is empty and solution hasn't changed.
                    // https://github.com/github/VisualStudio/issues/1421
                    log.Information("Ignoring no ActiveRepository when solution hasn't changed");
                }
                else
                {
                    if (newRepositoryPath != repositoryPath)
                    {
                        log.Information("Fire PropertyChanged event for ActiveRepository");
                        ActiveRepository = CreateRepository(newRepositoryPath);
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

        static string FindTrackedSha(string repositoryPath)
        {
            try
            {
                using (var repo = new Repository(repositoryPath))
                {
                    return repo.Head.TrackedBranch?.Tip.Sha;
                }
            }
            catch (RepositoryNotFoundException)
            {
                return null;
            }
        }

        ILocalRepositoryModel CreateRepository(string path)
        {
            if (path == null)
            {
                return null;
            }

            if (Splat.ModeDetector.InUnitTestRunner())
            {
                // HACK: This avoids calling GitService.GitServiceHelper.
                return new LocalRepositoryModel("testing", new UriString("github.com/testing/testing"), path);
            }

            return new LocalRepositoryModel(path);
        }

        static void FindActiveRepository(object gitExt, out string repositoryPath, out string branchName, out string headSha)
        {
            var activeRepositoriesProperty = gitExt.GetType().GetProperty("ActiveRepositories");
            var activeRepositories = (IEnumerable<object>)activeRepositoriesProperty?.GetValue(gitExt);
            var repo = activeRepositories?.FirstOrDefault();
            if (repo == null)
            {
                repositoryPath = null;
                branchName = null;
                headSha = null;
                return;
            }

            var repositoryPathProperty = repo.GetType().GetProperty("RepositoryPath");
            repositoryPath = (string)repositoryPathProperty?.GetValue(repo);

            var currentBranchProperty = repo.GetType().GetProperty("CurrentBranch");
            var currentBranch = currentBranchProperty?.GetValue(repo);

            var headShaProperty = currentBranch?.GetType().GetProperty("HeadSha");
            headSha = (string)headShaProperty?.GetValue(currentBranch);

            var nameProperty = currentBranch?.GetType().GetProperty("Name");
            branchName = (string)nameProperty?.GetValue(currentBranch);
        }

        public ILocalRepositoryModel ActiveRepository
        {
            get; private set;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler StatusChanged;
    }
}
