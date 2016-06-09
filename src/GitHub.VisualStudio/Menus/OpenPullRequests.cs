using GitHub.Exports;
using GitHub.UI;
using GitHub.VisualStudio.UI;
using NullGuard;
using System;
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio.Menus
{
    [ExportMenu(MenuType = MenuType.OpenPullRequests)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class OpenPullRequests: MenuBase, IMenuHandler
    {
        public Guid Guid => GuidList.guidGitHubCmdSet;
        public int CmdId => PkgCmdIDList.openPullRequestsCommand;

        public void Activate([AllowNull]object data = null)
        {
            var pane = GitHubPane.Activate();
            pane.ShowView(new ViewWithData(UIControllerFlow.PullRequests) { ViewType = UIViewType.PRList });
        }
    }
}
