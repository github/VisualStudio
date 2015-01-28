using Microsoft.TeamFoundation.Controls;
using GitHub.VisualStudio.Views;

namespace GitHub.VisualStudio
{
    [TeamExplorerSection(GitHubHomeSectionId, TeamExplorerPageIds.Home, 10)]
    class GitHubHomeSection : TeamExplorerSectionBase
    {
        public const string GitHubHomeSectionId = "72008232-2104-4FA0-A189-61B0C6F91198";

        public GitHubHomeSection()
            : base()
        {
            Title = "GitHub";
            // only when the repo is hosted on github.com
            IsVisible = true;
            IsExpanded = true;
            SectionContent = new GitHubHomeContent();
        }
    }

}


