using System;
using System.Linq;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Commands;
using GitHub.Logging;
using GitHub.Services;
using GitHub.Extensions;
using GitHub.Services.Vssdk.Commands;

namespace GitHub.VisualStudio.Commands
{
    /// <summary>
    /// Creates a GitHub Gist from the currently selected text.
    /// </summary>
    [Export(typeof(ICreateGistCommand))]
    public class CreateGistCommand : CreateGistCommandBase, ICreateGistCommand
    {
        [ImportingConstructor]
        protected CreateGistCommand(
            Lazy<IDialogService> dialogService,
            Lazy<ISelectedTextProvider> selectedTextProvider,
            Lazy<IConnectionManager> connectionManager)
            : base(CommandSet, CommandId, dialogService, selectedTextProvider, connectionManager, true,
                  isNotLoggedInDefault: true)
        {
        }

        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.guidContextMenuSet;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.createGistCommand;
    }

    /// <summary>
    /// Creates a GitHub Enterprise Gist from the currently selected text.
    /// </summary>
    [Export(typeof(ICreateGistEnterpriseCommand))]
    public class CreateGistEnterpriseCommand : CreateGistCommandBase, ICreateGistEnterpriseCommand
    {
        [ImportingConstructor]
        protected CreateGistEnterpriseCommand(
            Lazy<IDialogService> dialogService,
            Lazy<ISelectedTextProvider> selectedTextProvider,
            Lazy<IConnectionManager> connectionManager)
            : base(CommandSet, CommandId, dialogService, selectedTextProvider, connectionManager, false)
        {
        }

        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.guidContextMenuSet;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.createGistEnterpriseCommand;
    }

    /// <summary>
    /// Creates a GitHub or GitHub Enterprise Gist from the currently selected text.
    /// </summary>
    public abstract class CreateGistCommandBase : VsCommand
    {
        readonly bool isGitHubDotCom;
        readonly bool isNotLoggedInDefault;
        readonly Lazy<IDialogService> dialogService;
        readonly Lazy<ISelectedTextProvider> selectedTextProvider;
        readonly Lazy<IConnectionManager> connectionManager;

        protected CreateGistCommandBase(
            Guid commandSet, int commandId,
            Lazy<IDialogService> dialogService,
            Lazy<ISelectedTextProvider> selectedTextProvider,
            Lazy<IConnectionManager> connectionManager,
            bool isGitHubDotCom,
            bool isNotLoggedInDefault = false)
            : base(commandSet, commandId)
        {
            this.dialogService = dialogService;
            this.selectedTextProvider = selectedTextProvider;
            this.connectionManager = connectionManager;
            this.isGitHubDotCom = isGitHubDotCom;
            this.isNotLoggedInDefault = isNotLoggedInDefault;
        }

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
            Visible = !string.IsNullOrWhiteSpace(SelectedTextProvider?.GetSelectedText()) &&
                (HasConnection() || isNotLoggedInDefault && IsLoggedIn() == false);
        }

        bool HasConnection()
        {
            var task = FindConnectionAsync();
            return task.IsCompleted && task.Result != null;
        }

        async Task<IConnection> FindConnectionAsync()
        {
            var connections = await connectionManager.Value.GetLoadedConnections();
            return connections.FirstOrDefault(x => x.IsLoggedIn && x.HostAddress.IsGitHubDotCom() == isGitHubDotCom);
        }

        bool? IsLoggedIn()
        {
            var task = connectionManager.Value.IsLoggedIn();
            if (task.IsCompleted)
            {
                return task.Result;
            }

            return null;
        }
    }
}
