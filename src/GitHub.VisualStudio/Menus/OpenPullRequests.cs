using GitHub.Exports;
using GitHub.UI;
using GitHub.VisualStudio.UI;
using NullGuard;
using System;
using GitHub.Services;

namespace GitHub.VisualStudio.Menus
{
    [ExportMenu(MenuType = MenuType.OpenPullRequests)]
    public class OpenPullRequests : MenuBase, IMenuHandler
    {
        public OpenPullRequests(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public Guid Guid => GuidList.guidGitHubCmdSet;
        public int CmdId => PkgCmdIDList.openPullRequestsCommand;

        public void Activate([AllowNull]object data = null)
        {
            var host = ServiceProvider.TryGetService<IGitHubToolWindowManager>().ShowHomePane();
            host?.ShowView(new ViewWithData(UIControllerFlow.PullRequestList));
        }
    }
}
