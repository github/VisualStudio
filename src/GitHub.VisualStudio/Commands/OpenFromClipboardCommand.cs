using System;
using System.ComponentModel.Composition;
using GitHub.Commands;
using GitHub.Services;
using GitHub.Services.Vssdk.Commands;
using Task = System.Threading.Tasks.Task;

namespace GitHub.VisualStudio.Commands
{
    [Export(typeof(IOpenFromClipboardCommand))]
    public class OpenFromClipboardCommand : VsCommand<string>, IOpenFromClipboardCommand
    {
        public const string NoGitHubUrlMessage = "Couldn't a find a GitHub URL in clipboard";

        readonly Lazy<IGitHubContextService> gitHubContextService;
        readonly Lazy<ITeamExplorerContext> teamExplorerContext;
        readonly Lazy<IVSServices> vsServices;

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
            Lazy<IGitHubContextService> gitHubContextService,
            Lazy<ITeamExplorerContext> teamExplorerContext,
            Lazy<IVSServices> vsServices)
            : base(CommandSet, CommandId)
        {
            this.gitHubContextService = gitHubContextService;
            this.teamExplorerContext = teamExplorerContext;
            this.vsServices = vsServices;

            // See https://code.msdn.microsoft.com/windowsdesktop/AllowParams-2005-9442298f
            ParametersDescription = "u";    // accept a single url
        }

        public override Task Execute(string url)
        {
            var context = gitHubContextService.Value.FindContextFromClipboard();
            if (context == null)
            {
                vsServices.Value.ShowMessageBoxInfo(NoGitHubUrlMessage);
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
