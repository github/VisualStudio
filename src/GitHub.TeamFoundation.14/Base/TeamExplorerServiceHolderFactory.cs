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
        [Export(typeof(ITeamExplorerServiceHolder))]
        public ITeamExplorerServiceHolder TeamExplorerServiceHolder =>
            (ITeamExplorerServiceHolder)TeamFoundationResolver.Resolve(() => new TeamExplorerServiceHolder());
    }
}
