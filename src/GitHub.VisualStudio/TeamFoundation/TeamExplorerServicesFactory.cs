using System;
using System.ComponentModel.Composition;
using GitHub.TeamFoundation;

namespace GitHub.Services
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerServicesFactory
    {
        [ImportingConstructor]
        public TeamExplorerServicesFactory(TeamFoundationResolver teamFoundationResolver, IGitHubServiceProvider serviceProvider)
        {
            TeamExplorerServices = new TeamExplorerServices(serviceProvider);
        }

        [Export(typeof(ITeamExplorerServices))]
        public ITeamExplorerServices TeamExplorerServices { get; }
    }
}
