using Microsoft.TeamFoundation.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using NullGuard;
using GitHub.VisualStudio.Base;

namespace GitHub.VisualStudio
{
    class TeamExplorerNavigationItemBase : TeamExplorerItemBase, ITeamExplorerNavigationItem2, INotifyPropertySource
    {
        public TeamExplorerNavigationItemBase(IServiceProvider serviceProvider)
            : base()
        {
            this.ServiceProvider = serviceProvider;
        }

        int argbColor;
        public int ArgbColor
        {
            get { return argbColor; }
            set { argbColor = value; this.RaisePropertyChange(); }
        }

        object icon;
        [AllowNull]
        public object Icon
        {
            get { return icon; }
            set { icon = value; this.RaisePropertyChange(); }
        }

        Image image;
        [AllowNull]
        public Image Image
        {
            get { return image; }
            set { image = value; this.RaisePropertyChange(); }
        }

    }
}
