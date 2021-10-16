using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Commands;
using GitHub.Logging;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using GitHub.Services.Vssdk.Commands;
using Serilog;

namespace GitHub.VisualStudio.Commands
{
    /// <summary>
    /// Opens the GitHub pane and shows the currently checked out pull request.
    /// </summary>
    /// <remarks>
    /// Does nothing if there is no checked out pull request.
    /// </remarks>
    [Export(typeof(IShowCurrentPullRequestCommand))]
    public class ShowCurrentPullRequestCommand : VsCommand, IShowCurrentPullRequestCommand
    {
        static readonly ILogger log = LogManager.ForContext<ShowCurrentPullRequestCommand>();
        readonly IGitHubServiceProvider serviceProvider;

        [ImportingConstructor]
        protected ShowCurrentPullRequestCommand(IGitHubServiceProvider serviceProvider)
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
        public const int CommandId = PkgCmdIDList.showCurrentPullRequestCommand;

        /// <summary>
        /// Shows the current pull request.
        /// </summary>
        public override async Task Execute()
        {
            try
            {
                var pullRequestSessionManager = serviceProvider.ExportProvider.GetExportedValueOrDefault<IPullRequestSessionManager>();
                await pullRequestSessionManager.EnsureInitialized();

                var session = pullRequestSessionManager?.CurrentSession;
                if (session == null)
                {
                    return; // No active PR session.
                }

                var pullRequest = session.PullRequest;
                var manager = serviceProvider.TryGetService<IGitHubToolWindowManager>();
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
