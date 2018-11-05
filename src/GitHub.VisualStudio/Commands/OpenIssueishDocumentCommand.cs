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
    /// Opens an issue or pull request in a new document window.
    /// </summary>
    [Export(typeof(IOpenIssueishDocumentCommand))]
    public class OpenIssueishDocumentCommand : VsCommand<OpenIssueishParams>, IOpenIssueishDocumentCommand
    {
        static readonly ILogger log = LogManager.ForContext<ShowGitHubPaneCommand>();
        readonly IConnectionManager connectionManager;
        readonly IGitHubServiceProvider serviceProvider;

        [ImportingConstructor]
        protected OpenIssueishDocumentCommand(
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
        public const int CommandId = PkgCmdIDList.showIssueishDocumentCommand;

        /// <summary>
        /// Opens the issue or pull request.
        /// </summary>
        public override async Task Execute(OpenIssueishParams p)
        {
            try
            {
                var m = serviceProvider.GetService<IGitHubToolWindowManager>();
                var vm = await m.ShowIssueishDocumentPane(p.Address, p.Owner, p.Repository, p.Number);

                if (!vm.IsInitialized)
                {
                    var connection = await connectionManager.GetConnection(p.Address);
                    await vm.Load(connection, p.Owner, p.Repository, p.Number);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error opening issueish document pane");
            }
        }
    }
}

