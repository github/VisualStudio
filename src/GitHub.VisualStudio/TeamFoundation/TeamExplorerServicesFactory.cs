using System;
using System.ComponentModel.Composition;
using GitHub.TeamFoundation;

namespace GitHub.Services
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerServicesFactory
    {
        readonly Lazy<ITeamExplorerServices> teamExplorerServices;

        [ImportingConstructor]
        public TeamExplorerServicesFactory(IGitHubServiceProvider serviceProvider)
        {
            teamExplorerServices = new Lazy<ITeamExplorerServices>(() =>
                (ITeamExplorerServices)TeamFoundationResolver.Resolve(() => new TeamExplorerServices(serviceProvider)));
        }

        [Export(typeof(ITeamExplorerServices))]
        public ITeamExplorerServices TeamExplorerServices => teamExplorerServices.Value;
    }
}
