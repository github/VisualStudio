using GitHub.Api;
using GitHub.Models;
using GitHub.Services;
using GitHub.Settings;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio.TeamExplorer.Connect
{
    [TeamExplorerSection(GitHubConnectSection0Id, TeamExplorerPageIds.Connect, 10)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubConnectSection0 : GitHubConnectSection
    {
        public const string GitHubConnectSection0Id = "519B47D3-F2A9-4E19-8491-8C9FA25ABE90";

        [ImportingConstructor]
        public GitHubConnectSection0(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory,
            ITeamExplorerServiceHolder holder,
            IConnectionManager manager,
            IPackageSettings settings,
            IVSServices vsServices,
            ILocalRepositories localRepositories)
            : base(serviceProvider, apiFactory, holder, manager, settings, vsServices, localRepositories, 0)
        {
        }
    }
}
