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
            var newPath = FindActiveRepositoryPath(gitExt);
            var oldPath = ActiveRepository?.LocalPath;
            if (newPath != oldPath)
            {
                ActiveRepository = new LocalRepositoryModel(newPath);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveRepository)));
            }
            else
            {
                StatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        static string FindActiveRepositoryPath(object gitExt)
        {
            var activeRepositoriesProperty = gitExt.GetType().GetProperty("ActiveRepositories");
            var activeRepositories = (IEnumerable<object>)activeRepositoriesProperty?.GetValue(gitExt);
            if (activeRepositories == null)
            {
                return null;
            }

            var repo = activeRepositories.FirstOrDefault();
            if (repo == null)
            {
                return null;
            }

            var repositoryPathProperty = repo.GetType().GetProperty("RepositoryPath");
            var repositoryPath = (string)repositoryPathProperty?.GetValue(repo);
            return repositoryPath;
        }

        public ILocalRepositoryModel ActiveRepository
        {
            get; private set;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler StatusChanged;
    }
}
