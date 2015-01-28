using Microsoft.TeamFoundation.Controls;

namespace GitHub.VisualStudio
{
    [TeamExplorerSection(GitHubConnectSectionId, TeamExplorerPageIds.Connect, 10)]
    class GitHubConnectSection : TeamExplorerSectionBase
    {
        public const string GitHubConnectSectionId = "45081988-634A-4B8C-A4EA-9187169E567A";
        
        public GitHubConnectSection()
            : base()
        {
            Title = "GitHub";
            IsVisible = true;
            IsExpanded = true;
        }
    }
}
