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
        public TeamExplorerServiceHolderFactory()
        {
            TeamFoundationResolver.Resolve(
                () => typeof(Microsoft.VisualStudio.TeamFoundation.Git.Extensibility.IGitExt));

            TeamExplorerServiceHolder = (ITeamExplorerServiceHolder)TeamFoundationResolver.Resolve(
                () => new TeamExplorerServiceHolder());
        }

        [Export(typeof(ITeamExplorerServiceHolder))]
        public ITeamExplorerServiceHolder TeamExplorerServiceHolder { get; }
    }
}
