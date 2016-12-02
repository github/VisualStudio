using GitHub.VisualStudio.UI;
using NullGuard;
using System;

namespace GitHub.VisualStudio.Menus
{
    public class ShowGitHubPane: MenuBase, IMenuHandler
    {
        public Guid Guid => GuidList.guidGitHubCmdSet;
        public int CmdId => PkgCmdIDList.showGitHubPaneCommand;

        public void Activate([AllowNull]object data = null)
        {
            GitHubPane.Activate();
        }
    }
}
