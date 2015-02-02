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
    [TeamExplorerNavigationItem(IssuesNavigationItemId, 30, TargetPageId = TeamExplorerPageIds.Home)]
    class IssuesNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string IssuesNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA4";

        [ImportingConstructor]
        public IssuesNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Text = "Issues";
            IsVisible = true;
            IsEnabled = true;

            Image = UI.Controls.NavigationIcon.GetImage(ServiceProvider, GitHub.UI.Octicon.issue_closed);
        }
    }
}
