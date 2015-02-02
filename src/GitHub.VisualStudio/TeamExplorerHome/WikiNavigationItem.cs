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
    [TeamExplorerNavigationItem(WikiNavigationItemId, 10, TargetPageId = TeamExplorerPageIds.Home)]
    class WikiNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string WikiNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA1";

        [ImportingConstructor]
        public WikiNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Text = "Wiki";
            IsVisible = true;
            IsEnabled = true;

            Image = UI.Controls.NavigationIcon.GetImage(ServiceProvider, GitHub.UI.Octicon.book);

        }
    }
}
