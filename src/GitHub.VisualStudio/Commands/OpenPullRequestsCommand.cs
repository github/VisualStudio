using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Commands;
using GitHub.Logging;
using GitHub.Services;
using GitHub.Services.Vssdk.Commands;
using Serilog;

namespace GitHub.VisualStudio.Commands
{
    /// <summary>
    /// Opens the GitHub pane and shows the pull request list.
    /// </summary>
    [Export(typeof(IOpenPullRequestsCommand))]
    public class OpenPullRequestsCommand : VsCommand, IOpenPullRequestsCommand
    {
        static readonly ILogger log = LogManager.ForContext<OpenPullRequestsCommand>();
        readonly IGitHubServiceProvider serviceProvider;

        [ImportingConstructor]
        protected OpenPullRequestsCommand(IGitHubServiceProvider serviceProvider)
            : base(CommandSet, CommandId)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.guidGitHubCmdSet;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.openPullRequestsCommand;

        /// <summary>
        /// Opens the GitHub pane and shows the pull request list.
        /// </summary>
        public override async Task Execute()
        {
            try
            {
                var host = await serviceProvider.TryGetMEFComponent<IGitHubToolWindowManager>().ShowGitHubPane();
                await host.ShowPullRequests();
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error showing opening pull requests");
            }
        }
    }
}
