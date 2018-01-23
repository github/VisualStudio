using System;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Services;
using Serilog;

namespace GitHub.VisualStudio.Menus
{
    [ExportMenu(MenuType = MenuType.OpenPullRequests)]
    public class ShowCurrentPullRequest : MenuBase, IMenuHandler
    {
        static readonly ILogger log = LogManager.ForContext<ShowCurrentPullRequest>();

        public ShowCurrentPullRequest(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
        }

        public Guid Guid => Guids.guidGitHubCmdSet;
        public int CmdId => PkgCmdIDList.showCurrentPullRequestCommand;

        public async void Activate(object data = null)
        {
            try
            {
                var pullRequestSessionManager = ServiceProvider.ExportProvider.GetExportedValueOrDefault<IPullRequestSessionManager>();
                var session = pullRequestSessionManager?.CurrentSession;
                if (session == null)
                {
                    return; // No active PR session.
                }

                var pullRequest = session.PullRequest;
                var manager = ServiceProvider.TryGetService<IGitHubToolWindowManager>();
                var host = await manager.ShowGitHubPane();
                await host.ShowPullRequest(session.RepositoryOwner, host.LocalRepository.Name, pullRequest.Number);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error showing current pull request");
            }
        }
    }
}
