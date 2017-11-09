using System;
using GitHub.Exports;
using GitHub.UI;
using GitHub.Services;
using GitHub.Extensions;
using GitHub.Models;

namespace GitHub.VisualStudio.Menus
{
    [ExportMenu(MenuType = MenuType.OpenPullRequests)]
    public class ShowCurrentPullRequest : MenuBase, IMenuHandler
    {
        public ShowCurrentPullRequest(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
        }

        public Guid Guid => Guids.guidGitHubCmdSet;
        public int CmdId => PkgCmdIDList.showCurrentPullRequestCommand;

        public void Activate(object data = null)
        {
            var pullRequestSessionManager = ServiceProvider.ExportProvider.GetExportedValueOrDefault<IPullRequestSessionManager>();
            var session = pullRequestSessionManager?.CurrentSession;
            if (session == null)
            {
                return; // No active PR session.
            }

            var pullRequest = session.PullRequest;
            var arg = new PullRequestDetailArgument { RepositoryOwner = session.RepositoryOwner, Number = pullRequest.Number };
            var viewWithData = new ViewWithData(UIControllerFlow.PullRequestDetail) { Data = arg };

            var manager = ServiceProvider.TryGetService<IGitHubToolWindowManager>();
            var host = manager.ShowHomePane();
            host?.ShowView(viewWithData);
        }
    }
}
