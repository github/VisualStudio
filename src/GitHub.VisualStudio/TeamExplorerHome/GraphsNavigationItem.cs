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
    [TeamExplorerNavigationItem(GraphsNavigationItemId, 10, TargetPageId = TeamExplorerPageIds.Home)]
    class GraphsNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string GraphsNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA5";

        [ImportingConstructor]
        public GraphsNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Text = "Graphs";
            IsVisible = true;
            IsEnabled = true;

            Image = UI.Controls.NavigationIcon.GetImage(ServiceProvider, GitHub.UI.Octicon.graph);
        }
    }
}
