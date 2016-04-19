using GitHub.Exports;
using GitHub.VisualStudio.UI;
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

        public void Activate()
        {
            GitHubPane.Activate();
        }
    }
}
