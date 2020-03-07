using System;
using System.ComponentModel.Composition;
using GitHub.Commands;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.Services.Vssdk.Commands;
using Task = System.Threading.Tasks.Task;

namespace GitHub.VisualStudio.Commands
{
    [Export(typeof(IOpenFromUrlCommand))]
    public class OpenFromUrlCommand : VsCommand<string>, IOpenFromUrlCommand
    {
        readonly Lazy<IDialogService> dialogService;
        readonly Lazy<IRepositoryCloneService> repositoryCloneService;
        readonly Lazy<ITeamExplorerContext> teamExplorerContext;
        readonly Lazy<IGitHubContextService> gitHubContextService;

        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.guidGitHubCmdSet;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.openFromUrlCommand;

        [ImportingConstructor]
        public OpenFromUrlCommand(
            Lazy<IDialogService> dialogService,
            Lazy<IRepositoryCloneService> repositoryCloneService,
            Lazy<ITeamExplorerContext> teamExplorerContext,
            Lazy<IGitHubContextService> gitHubContextService) :
            base(CommandSet, CommandId)
        {
            this.dialogService = dialogService;
            this.repositoryCloneService = repositoryCloneService;
            this.teamExplorerContext = teamExplorerContext;
            this.gitHubContextService = gitHubContextService;

            // See https://code.msdn.microsoft.com/windowsdesktop/AllowParams-2005-9442298f
            ParametersDescription = "u";    // accept a single url
        }

        public override async Task Execute(string targetUrl)
        {
            if (targetUrl != null)
            {
                // Navigate to  to active repository when same as target
                if (FindActiveRepositoryDir(targetUrl) is string repositoryDir)
                {
                    // Navigate to context for supported URL types (e.g. /blob/ URLs)
                    if (gitHubContextService.Value.FindContextFromUrl(targetUrl) is GitHubContext context)
                    {
                        gitHubContextService.Value.TryNavigateToContext(repositoryDir, context);
                        return;
                    }
                }
            }

            var cloneDialogResult = await dialogService.Value.ShowCloneDialog(null, targetUrl);
            if (cloneDialogResult != null)
            {
                await repositoryCloneService.Value.CloneOrOpenRepository(cloneDialogResult);
            }
        }

        string FindActiveRepositoryDir(UriString targetUrl)
        {
            if (teamExplorerContext.Value.ActiveRepository is LocalRepositoryModel activeRepository)
            {
                if (activeRepository.CloneUrl is UriString activeUrl)
                {
                    if (UriString.RepositoryUrlsAreEqual(activeUrl, targetUrl))
                    {
                        return activeRepository.LocalPath;
                    }
                }
            }

            return null;
        }
    }
}
