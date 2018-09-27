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
    /// Opens the issue details pane with the specified issue.
    /// </summary>
    [Export(typeof(IShowIssueDetailsCommand))]
    public class ShowIssueDetailsCommand : VsCommand<ShowIssueDetailsParams>, IShowIssueDetailsCommand
    {
        static readonly ILogger log = LogManager.ForContext<ShowGitHubPaneCommand>();
        readonly IConnectionManager connectionManager;
        readonly IGitHubServiceProvider serviceProvider;

        [ImportingConstructor]
        protected ShowIssueDetailsCommand(
            IConnectionManager connectionManager,
            IGitHubServiceProvider serviceProvider)
            : base(CommandSet, CommandId)
        {
            this.connectionManager = connectionManager;
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.guidGitHubCmdSet;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.showGitHubPaneCommand;

        /// <summary>
        /// Shows the GitHub pane.
        /// </summary>
        public override async Task Execute(ShowIssueDetailsParams p)
        {
            try
            {
                var m = serviceProvider.GetService<IGitHubToolWindowManager>();
                var vm = await m.ShowIssueDetailPane();
                var connection = await connectionManager.GetConnection(p.Address);
                await vm.InitializeAsync(connection, p.Owner, p.Repository, p.Number);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error showing opening Issue Details pane");
            }
        }
    }
}
