using GitHub.Models;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio.TeamExplorerConnect
{
    [TeamExplorerSection(GitHubConnectSection2Id, TeamExplorerPageIds.Connect, 10)]
    public class GitHubConnectSection2 : GitHubConnectSection
    {
        public const string GitHubConnectSection2Id = "519B47D3-F2A9-4E19-8491-8C9FA25ABE92";

        [ImportingConstructor]
        public GitHubConnectSection2(IConnectionManager manager) : base(manager, 2)
        {
        }
    }
}
