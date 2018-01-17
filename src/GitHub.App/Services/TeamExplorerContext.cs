using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using GitHub.Models;
using GitHub.Logging;
using Serilog;

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

        string repositoryPath;
        string branchName;
        string headSha;

        [ImportingConstructor]
        public TeamExplorerContext([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            var gitExtType = Type.GetType(GitExtTypeName, false);
            if (gitExtType == null)
            {
                log.Error("Couldn't find type {GitExtTypeName}", GitExtTypeName);
                return;
            }

            var gitExt = serviceProvider.GetService(gitExtType);
            if (gitExt == null)
            {
                log.Error("Couldn't find service for type {GitExtType}", gitExtType);
                return;
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

        void Refresh(object gitExt)
        {
            string newRepositoryPath;
            string newBranchName;
            string newHeadSha;
            FindActiveRepository(gitExt, out newRepositoryPath, out newBranchName, out newHeadSha);

            log.Information("Refresh ActiveRepository: RepositoryPath={RepositoryPath}, BranchName={BranchName}, HeadSha={HeadSha}",
                newRepositoryPath, newBranchName, newHeadSha);

            if (newRepositoryPath == null)
            {
                // Ignore when ActiveRepositories is empty.
                // The ActiveRepository can be changed but not unloaded.
                // https://github.com/github/VisualStudio/issues/1421
                log.Information("Ignoring null ActiveRepository");
            }
            else if (newRepositoryPath != repositoryPath)
            {
                log.Information("Fire PropertyChanged event for ActiveRepository");

                repositoryPath = newRepositoryPath;
                branchName = newBranchName;
                headSha = newHeadSha;

                ActiveRepository = repositoryPath != null ? new LocalRepositoryModel(repositoryPath) : null;
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
