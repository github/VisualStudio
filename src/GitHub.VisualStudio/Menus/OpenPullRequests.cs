using GitHub.Exports;
using GitHub.UI;
using GitHub.VisualStudio.UI;
using System;
using GitHub.Services;
using GitHub.Extensions;

namespace GitHub.VisualStudio.Menus
{
    [ExportMenu(MenuType = MenuType.OpenPullRequests)]
    public class OpenPullRequests : MenuBase, IMenuHandler
    {
        public OpenPullRequests(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
        }

        public Guid Guid => GuidList.guidGitHubCmdSet;
        public int CmdId => PkgCmdIDList.openPullRequestsCommand;

        public void Activate(object data = null)
        {
            var host = ServiceProvider.TryGetService<IGitHubToolWindowManager>().ShowHomePane();
            host?.ShowView(new ViewWithData(UIControllerFlow.PullRequestList));
        }
    }
}
