using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.Composition;
using GitHub.Models;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;

namespace GitHub.Services
{
    /// <summary>
    /// This service uses reflection to access the IGitExt from Visual Studio 2015 and 2017.
    /// </summary>
    [Export(typeof(ITeamExplorerContext))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerContext : ITeamExplorerContext
    {
        [ImportingConstructor]
        public TeamExplorerContext([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            var gitExtType = Type.GetType("Microsoft.VisualStudio.TeamFoundation.Git.Extensibility.IGitExt, Microsoft.TeamFoundation.Git.Provider", false);
            if (gitExtType != null)
            {
                var gitExt = serviceProvider.GetService(gitExtType);
                Refresh(gitExt);

                var notifyPropertyChanged = (INotifyPropertyChanged)gitExt;
                notifyPropertyChanged.PropertyChanged += (s, e) => Refresh(gitExt);
            }
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
            var activeRepositories = (IEnumerable<object>)activeRepositoriesProperty.GetValue(gitExt);
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
            var repositoryPath = (string)repositoryPathProperty.GetValue(repo);
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
