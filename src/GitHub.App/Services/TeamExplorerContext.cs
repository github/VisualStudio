using System;
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

        [ImportingConstructor]
        public TeamExplorerContext(ITeamExplorerServiceHolder teamExplorerServiceHolder)
        {
            this.teamExplorerServiceHolder = teamExplorerServiceHolder;

            teamExplorerServiceHolder.Subscribe(this, repo =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveRepository)));
            });

            teamExplorerServiceHolder.StatusChanged += (s, e) =>
            {
                StatusChanged?.Invoke(this, e);
            };
        }

        public ILocalRepositoryModel ActiveRepository => teamExplorerServiceHolder.ActiveRepo;

        public event EventHandler StatusChanged;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
