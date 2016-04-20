using GitHub.Exports;
using GitHub.UI;
using GitHub.VisualStudio.UI;
using NullGuard;
using System;
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio.Menus
{
    [ExportMenu(MenuType = MenuType.GitHubPane)]
    [PartCreationPolicy(CreationPolicy.Shared)]
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
