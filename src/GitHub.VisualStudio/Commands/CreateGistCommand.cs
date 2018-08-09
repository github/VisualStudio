using System;
using System.Linq;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Commands;
using GitHub.Logging;
using GitHub.Services;
using GitHub.Services.Vssdk.Commands;

namespace GitHub.VisualStudio.Commands
{
    /// <summary>
    /// Creates a gist from the currently selected text.
    /// </summary>
    [Export(typeof(ICreateGistCommand))]
    public class CreateGistCommand : VsCommand, ICreateGistCommand
    {
        readonly Lazy<IDialogService> dialogService;
        readonly Lazy<ISelectedTextProvider> selectedTextProvider;
        readonly Lazy<IConnectionManager> connectionManager;

        [ImportingConstructor]
        protected CreateGistCommand(
            Lazy<IDialogService> dialogService,
            Lazy<ISelectedTextProvider> selectedTextProvider,
            Lazy<IConnectionManager> connectionManager)
            : base(CommandSet, CommandId)
        {
            this.dialogService = dialogService;
            this.selectedTextProvider = selectedTextProvider;
            this.connectionManager = connectionManager;
        }

        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.guidContextMenuSet;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.createGistCommand;

        ISelectedTextProvider SelectedTextProvider => selectedTextProvider.Value;

        /// <summary>
        /// Shows the Create Gist dialog.
        /// </summary>
        public override async Task Execute()
        {
            var connection = await FindConnectionAsync();
            await dialogService.Value.ShowCreateGist(connection);
        }

        protected override void QueryStatus()
        {
            Log.Assert(SelectedTextProvider != null, "Could not get an instance of ISelectedTextProvider");
            Visible = !string.IsNullOrWhiteSpace(SelectedTextProvider?.GetSelectedText()) && HasConnection();
        }

        bool HasConnection()
        {
            var task = FindConnectionAsync();
            return task.IsCompleted && task.Result != null;
        }

        async Task<IConnection> FindConnectionAsync()
        {
            var connections = await connectionManager.Value.GetLoadedConnections();
            return connections.FirstOrDefault(x => x.IsLoggedIn && x.HostAddress.IsGitHubDotCom());
        }
    }
}
