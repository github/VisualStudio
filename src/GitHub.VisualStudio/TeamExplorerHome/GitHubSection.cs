using GitHub.VisualStudio.Base;
using Microsoft.TeamFoundation.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.VisualStudio.TeamExplorerHome
{
    [TeamExplorerSection("72008232-2104-4FA0-A189-61B0C6F91198", Microsoft.TeamFoundation.Controls.TeamExplorerPageIds.Home, 10)]
    class GitHubSection : TeamExplorerSectionBase
    {
        public GitHubSection()
            : base()
        {
            Title = "GitHub";
            IsVisible = true;
            IsExpanded = true;
        }

    }
}
