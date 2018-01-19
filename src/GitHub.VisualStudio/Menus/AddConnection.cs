using GitHub.Services;
using System;

namespace GitHub.VisualStudio.Menus
{
    public class AddConnection : MenuBase, IMenuHandler
    {
        public AddConnection(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public Guid Guid => Guids.guidGitHubCmdSet;
        public int CmdId => PkgCmdIDList.addConnectionCommand;

        public void Activate(object data = null)
        {
            DialogService?.ShowLoginDialog();
        }
    }
}
