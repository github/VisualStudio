using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.Composition;
using GitHub.Models;

namespace GitHub.Services
{
    [Export(typeof(ITeamExplorerContext))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerContext : ITeamExplorerContext
    {
        readonly ITeamExplorerServiceHolder teamExplorerServiceHolder;

        ILocalRepositoryModel activeRepository;

        [ImportingConstructor]
        public TeamExplorerContext(IVSGitExt vsGitExt, ITeamExplorerServiceHolder teamExplorerServiceHolder)
        {
            this.teamExplorerServiceHolder = teamExplorerServiceHolder;

            vsGitExt.ActiveRepositoriesChanged += () =>
            {
                StatusChanged?.Invoke(this, EventArgs.Empty);
            };

            teamExplorerServiceHolder.Subscribe(this, repo =>
            {
                if (!Equals(repo, activeRepository))
                {
                    activeRepository = repo;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveRepository)));
                }
            });
        }

        public ILocalRepositoryModel ActiveRepository => teamExplorerServiceHolder.ActiveRepo;

        public event EventHandler StatusChanged;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
