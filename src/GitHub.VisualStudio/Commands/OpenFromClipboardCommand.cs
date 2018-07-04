using System;
using System.Windows;
using System.ComponentModel.Composition;
using GitHub.Commands;
using GitHub.Services;
using GitHub.App.Services;
using GitHub.Services.Vssdk.Commands;
using Task = System.Threading.Tasks.Task;

namespace GitHub.VisualStudio.Commands
{
    [Export(typeof(IOpenFromClipboardCommand))]
    public class OpenFromClipboardCommand : VsCommand<string>, IOpenFromClipboardCommand
    {
        readonly Lazy<GitHubContextService> gitHubContextService;
        readonly Lazy<ITeamExplorerContext> teamExplorerContext;

        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.guidGitHubCmdSet;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.openFromClipboardCommand;

        [ImportingConstructor]
        public OpenFromClipboardCommand(
            Lazy<GitHubContextService> gitHubContextService,
            Lazy<ITeamExplorerContext> teamExplorerContext)
            : base(CommandSet, CommandId)
        {
            this.gitHubContextService = gitHubContextService;
            this.teamExplorerContext = teamExplorerContext;

            // See https://code.msdn.microsoft.com/windowsdesktop/AllowParams-2005-9442298f
            ParametersDescription = "u";    // accept a single url
        }

        public override Task Execute(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                url = Clipboard.GetText(TextDataFormat.Text);
            }

            var context = gitHubContextService.Value.FindContextFromUrl(url);
            if (context == null)
            {
                // Couldn't find URL in clipboard
                return Task.CompletedTask;
            }

            var activeDir = teamExplorerContext.Value.ActiveRepository?.LocalPath;
            if (context == null)
            {
                // No active repository
                return Task.CompletedTask;
            }

            if (!gitHubContextService.Value.TryOpenFile(activeDir, context))
            {
                // Couldn't open file
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}
