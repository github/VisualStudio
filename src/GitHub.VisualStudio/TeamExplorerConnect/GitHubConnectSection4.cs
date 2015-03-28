using GitHub.Models;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio.TeamExplorerConnect
{
    [TeamExplorerSection(GitHubConnectSection4Id, TeamExplorerPageIds.Connect, 10)]
    public class GitHubConnectSection4 : GitHubConnectSection
    {
        public const string GitHubConnectSection4Id = "519B47D3-F2A9-4E19-8491-8C9FA25ABE94";

        [ImportingConstructor]
        public GitHubConnectSection4(IConnectionManager manager) : base(manager, 4)
        {
        }
    }
}
