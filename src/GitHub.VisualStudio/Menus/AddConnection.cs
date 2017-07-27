using GitHub.Services;
using GitHub.UI;
using System;

namespace GitHub.VisualStudio.Menus
{
    public class AddConnection: MenuBase, IMenuHandler
    {
        public AddConnection(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public Guid Guid => GuidList.guidGitHubCmdSet;
        public int CmdId => PkgCmdIDList.addConnectionCommand;

        public void Activate(object data = null)
        {
            StartFlow(UIControllerFlow.Authentication);
        }
    }
}
