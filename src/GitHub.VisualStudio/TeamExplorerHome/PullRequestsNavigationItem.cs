using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio
{
    [TeamExplorerNavigationItem(PullRequestsNavigationItemId, 10, TargetPageId = TeamExplorerPageIds.Home)]
    class PullRequestsNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string PullRequestsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA3";

        [ImportingConstructor]
        public PullRequestsNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Text = "Pull Requests";
            IsVisible = true;
            IsEnabled = true;

            Image = UI.Controls.NavigationIcon.GetImage(ServiceProvider, GitHub.UI.Octicon.git_pull_request);
        }
    }
}
