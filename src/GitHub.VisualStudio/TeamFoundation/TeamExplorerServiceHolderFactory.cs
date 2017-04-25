using System;
using System.ComponentModel.Composition;
using GitHub.Services;
using GitHub.TeamFoundation;

namespace GitHub.VisualStudio.Base
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerServiceHolderFactory
    {
        [ImportingConstructor]
        public TeamExplorerServiceHolderFactory(TeamFoundationResolver teamFoundationResolver)
        {
            TeamExplorerServiceHolder = new TeamExplorerServiceHolder();
        }

        [Export(typeof(ITeamExplorerServiceHolder))]
        public ITeamExplorerServiceHolder TeamExplorerServiceHolder { get; }
    }
}
