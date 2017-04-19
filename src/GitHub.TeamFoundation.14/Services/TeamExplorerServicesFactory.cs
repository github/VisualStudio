using System;
using System.ComponentModel.Composition;
using GitHub.TeamFoundation;

namespace GitHub.Services
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TeamExplorerServicesFactory
    {
        readonly IGitHubServiceProvider serviceProvider;

        [ImportingConstructor]
        public TeamExplorerServicesFactory(IGitHubServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [Export(typeof(ITeamExplorerServices))]
        public ITeamExplorerServices TeamExplorerServices =>
            (ITeamExplorerServices)TeamFoundationResolver.Resolve(() => new TeamExplorerServices(serviceProvider));
    }
}
