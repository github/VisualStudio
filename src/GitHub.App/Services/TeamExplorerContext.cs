using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using GitHub.Models;
using GitHub.Logging;
using GitHub.Primitives;
using Serilog;
using EnvDTE;

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
        static readonly ILogger log = LogManager.ForContext<TeamExplorerContext>();

        readonly DTE dte;

        string solutionPath;
        string repositoryPath;
        string branchName;
        string headSha;
        bool testing;

        [ImportingConstructor]
        public TeamExplorerContext([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : this(serviceProvider, FindGitExtType(), false)
        {
        }

        public TeamExplorerContext([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider, Type gitExtType, bool testing)
        {
            this.testing = testing;

            var gitExt = serviceProvider.GetService(gitExtType);
            if (gitExt == null)
            {
                log.Error("Couldn't find service for type {GitExtType}", gitExtType);
                return;
            }

            dte = (DTE)serviceProvider.GetService(typeof(DTE));
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

        static Type FindGitExtType()
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
            string newRepositoryPath;
            string newBranchName;
            string newHeadSha;
            FindActiveRepository(gitExt, out newRepositoryPath, out newBranchName, out newHeadSha);
            var newSolutionPath = dte?.Solution?.FullName;

            log.Information("Refresh ActiveRepository: RepositoryPath={RepositoryPath}, BranchName={BranchName}, HeadSha={HeadSha}, SolutionPath={SolutionPath}",
                newRepositoryPath, newBranchName, newHeadSha, newSolutionPath);

            if (newRepositoryPath == null && newSolutionPath == solutionPath)
            {
                // Ignore when ActiveRepositories is empty and solution hasn't changed.
                // https://github.com/github/VisualStudio/issues/1421
                log.Information("Ignoring null ActiveRepository");
            }
            else if (newRepositoryPath != repositoryPath)
            {
                log.Information("Fire PropertyChanged event for ActiveRepository");

                solutionPath = newSolutionPath;
                repositoryPath = newRepositoryPath;
                branchName = newBranchName;
                headSha = newHeadSha;

                ActiveRepository = CreateRepository(repositoryPath);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveRepository)));
            }
            else if (newBranchName != branchName || newHeadSha != headSha)
            {
                log.Information("Fire StatusChanged event for ActiveRepository");

                branchName = newBranchName;
                headSha = newHeadSha;

                StatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        ILocalRepositoryModel CreateRepository(string repositoryPath)
        {
            if (repositoryPath == null)
            {
                return null;
            }

            if (testing)
            {
                // HACK: This avoids calling GitService.GitServiceHelper.
                return new LocalRepositoryModel("testing", new UriString("github.com/testing/testing"), repositoryPath);
            }

            return new LocalRepositoryModel(repositoryPath);
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
