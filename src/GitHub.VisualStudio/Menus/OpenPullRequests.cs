using System;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Services;
using Serilog;

namespace GitHub.VisualStudio.Menus
{
    [ExportMenu(MenuType = MenuType.OpenPullRequests)]
    public class OpenPullRequests : MenuBase, IMenuHandler
    {
        static readonly ILogger log = LogManager.ForContext<ShowCurrentPullRequest>();

        public OpenPullRequests(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
        }

        public Guid Guid => Guids.guidGitHubCmdSet;
        public int CmdId => PkgCmdIDList.openPullRequestsCommand;

        public async void Activate(object data = null)
        {
            try
            {
                var host = await ServiceProvider.TryGetService<IGitHubToolWindowManager>().ShowGitHubPane();
                await host.ShowPullRequests();
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error showing opening pull requests");
            }
        }
    }
}
