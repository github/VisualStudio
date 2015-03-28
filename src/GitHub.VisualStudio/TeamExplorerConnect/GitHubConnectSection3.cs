using GitHub.Models;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio.TeamExplorerConnect
{
    [TeamExplorerSection(GitHubConnectSection3Id, TeamExplorerPageIds.Connect, 10)]
    public class GitHubConnectSection3 : GitHubConnectSection
    {
        public const string GitHubConnectSection3Id = "519B47D3-F2A9-4E19-8491-8C9FA25ABE93";

        [ImportingConstructor]
        public GitHubConnectSection3(IConnectionManager manager) : base(manager, 3)
        {
        }
    }
}
