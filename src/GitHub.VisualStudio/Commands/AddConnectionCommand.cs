using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Commands;
using GitHub.Services;
using GitHub.Services.Vssdk.Commands;

namespace GitHub.VisualStudio.Commands
{
    /// <summary>
    /// Opens the login dialog to add a new connection to Team Explorer.
    /// </summary>
    [Export(typeof(IAddConnectionCommand))]
    public class AddConnectionCommand : VsCommand, IAddConnectionCommand
    {
        readonly Lazy<IDialogService> dialogService;

        [ImportingConstructor]
        protected AddConnectionCommand(IGitHubServiceProvider serviceProvider)
            : base(CommandSet, CommandId)
        {
            dialogService = new Lazy<IDialogService>(() => serviceProvider.TryGetMEFComponent<IDialogService>());
        }

        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.guidGitHubCmdSet;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.addConnectionCommand;

        /// <summary>
        /// Shows the login dialog.
        /// </summary>
        public override Task Execute()
        {
            return dialogService.Value.ShowLoginDialog();
        }
    }
}
