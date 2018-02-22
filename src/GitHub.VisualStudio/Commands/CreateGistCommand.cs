using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
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
        readonly IDialogService dialogService;
        readonly Lazy<ISelectedTextProvider> selectedTextProvider;

        [ImportingConstructor]
        protected CreateGistCommand(
            IDialogService dialogService,
            IGitHubServiceProvider serviceProvider)
            : base(CommandSet, CommandId)
        {
            this.dialogService = dialogService;
            selectedTextProvider = new Lazy<ISelectedTextProvider>(() => serviceProvider.TryGetService<ISelectedTextProvider>());
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
        public override Task Execute()
        {
            return dialogService.ShowCreateGist();
        }

        protected override void QueryStatus()
        {
            Log.Assert(SelectedTextProvider != null, "Could not get an instance of ISelectedTextProvider");
            Visible = !string.IsNullOrWhiteSpace(SelectedTextProvider?.GetSelectedText());
        }
    }
}
