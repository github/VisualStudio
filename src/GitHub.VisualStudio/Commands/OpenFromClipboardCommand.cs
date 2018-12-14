using System;
using System.ComponentModel.Composition;
using System.Globalization;
using GitHub.Commands;
using GitHub.Exports;
using GitHub.Services;
using GitHub.Services.Vssdk.Commands;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitHub.VisualStudio.Commands
{
    [Export(typeof(IOpenFromClipboardCommand))]
    public class OpenFromClipboardCommand : VsCommand<string>, IOpenFromClipboardCommand
    {
        readonly Lazy<IGitHubContextService> gitHubContextService;
        readonly Lazy<ITeamExplorerContext> teamExplorerContext;
        readonly Lazy<IVSServices> vsServices;
        readonly Lazy<IGitService> gitService;
        readonly Lazy<UIContext> uiContext;

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
            Lazy<IVSServices> vsServices,
            Lazy<IGitService> gitService)
            : base(CommandSet, CommandId)
        {
            this.gitHubContextService = gitHubContextService;
            this.teamExplorerContext = teamExplorerContext;
            this.vsServices = vsServices;
            this.gitService = gitService;

            // See https://code.msdn.microsoft.com/windowsdesktop/AllowParams-2005-9442298f
            ParametersDescription = "u";    // accept a single url

            // This command is only visible when in the context of a Git repository
            uiContext = new Lazy<UIContext>(() => UIContext.FromUIContextGuid(new Guid(Guids.GitContextPkgString)));
        }

        public override async Task Execute(string url)
        {
            var context = gitHubContextService.Value.FindContextFromClipboard();
            if (context == null)
            {
                vsServices.Value.ShowMessageBoxInfo(Resources.NoGitHubUrlMessage);
                return;
            }

            if (context.LinkType != LinkType.Blob)
            {
                var message = string.Format(CultureInfo.CurrentCulture, Resources.UnknownLinkTypeMessage, context.Url);
                vsServices.Value.ShowMessageBoxInfo(message);
                return;
            }

            var activeRepository = teamExplorerContext.Value.ActiveRepository;
            var repositoryDir = activeRepository?.LocalPath;
            if (repositoryDir == null)
            {
                vsServices.Value.ShowMessageBoxInfo(Resources.NoActiveRepositoryMessage);
                return;
            }

            if (!string.Equals(activeRepository.Name, context.RepositoryName, StringComparison.OrdinalIgnoreCase))
            {
                vsServices.Value.ShowMessageBoxInfo(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.DifferentRepositoryMessage,
                        context.RepositoryName));
                return;
            }

            var (commitish, path, isSha) = gitHubContextService.Value.ResolveBlob(repositoryDir, context);
            if (path == null)
            {
                if (!string.Equals(activeRepository.Owner, context.Owner, StringComparison.OrdinalIgnoreCase))
                {
                    vsServices.Value.ShowMessageBoxInfo(Resources.NoResolveDifferentOwnerMessage);
                }
                else
                {
                    vsServices.Value.ShowMessageBoxInfo(Resources.NoResolveSameOwnerMessage);
                }

                return;
            }

            var hasChanges = gitHubContextService.Value.HasChangesInWorkingDirectory(repositoryDir, commitish, path);
            if (hasChanges)
            {
                // TODO: What if this returns null because we're not on a branch?
                var currentBranch = gitService.Value.GetBranch(activeRepository);
                var branchName = currentBranch.Name;

                // AnnotateFile expects a branch name so we use the current branch
                if (await gitHubContextService.Value.TryAnnotateFile(repositoryDir, branchName, context))
                {
                    return;
                }

                vsServices.Value.ShowMessageBoxInfo(Resources.ChangesInWorkingDirectoryMessage);
            }

            gitHubContextService.Value.TryOpenFile(repositoryDir, context);
        }

        protected override void QueryStatus()
        {
            Visible = uiContext.Value.IsActive;
        }
    }
}
