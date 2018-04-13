using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Commands;
using GitHub.Logging;
using GitHub.Services;
using GitHub.Services.Vssdk.Commands;
using Microsoft.VisualStudio.Threading;

namespace GitHub.VisualStudio.Commands
{
    /// <summary>
    /// Creates a gist from the currently selected text.
    /// </summary>
    [Export(typeof(ICreateGistCommand))]
    public class CreateGistCommand : VsCommand, ICreateGistCommand
    {
        readonly IDialogService dialogService;
        readonly ISelectedTextProvider selectedTextProvider;

        [ImportingConstructor]
        protected CreateGistCommand(IDialogService dialogService, ISelectedTextProvider selectedTextProvider)
            : base(CommandSet, CommandId)
        {
            this.dialogService = dialogService;
            this.selectedTextProvider = selectedTextProvider;
        }

        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.guidContextMenuSet;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.createGistCommand;

        /// <summary>
        /// Shows the Create Gist dialog.
        /// </summary>
        public override Task Execute()
        {
            return dialogService.ShowCreateGist();
        }

        protected override void QueryStatus()
        {
            Visible = !string.IsNullOrWhiteSpace(selectedTextProvider.GetSelectedText());
        }
    }
}
