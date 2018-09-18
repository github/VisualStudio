using System;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using GitHub.Commands;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using GitHub.Services.Vssdk.Commands;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using SVsServiceProvider = Microsoft.VisualStudio.Shell.SVsServiceProvider;
using GitHub.VisualStudio.UI;

namespace GitHub.VisualStudio.Commands
{
    [Export(typeof(IOpenFromUrlCommand))]
    public class OpenFromUrlCommand : VsCommand<string>, IOpenFromUrlCommand
    {
        readonly Lazy<IDialogService> dialogService;
        readonly Lazy<IGitHubContextService> gitHubContextService;
        readonly Lazy<IRepositoryCloneService> repositoryCloneService;
        readonly Lazy<IGitHubToolWindowManager> gitHubToolWindowManager;
        readonly Lazy<DTE> dte;

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
            Lazy<IGitHubContextService> gitHubContextService,
            Lazy<IRepositoryCloneService> repositoryCloneService,
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider) :
            base(CommandSet, CommandId)
        {
            this.dialogService = dialogService;
            this.gitHubContextService = gitHubContextService;
            this.repositoryCloneService = repositoryCloneService;
            dte = new Lazy<DTE>(() => (DTE)serviceProvider.GetService(typeof(DTE)));
            gitHubToolWindowManager = new Lazy<IGitHubToolWindowManager>(
                () => (IGitHubToolWindowManager)serviceProvider.GetService(typeof(IGitHubToolWindowManager)));

            // See https://code.msdn.microsoft.com/windowsdesktop/AllowParams-2005-9442298f
            ParametersDescription = "u";    // accept a single url
        }

        public override async Task Execute(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                var clipboardContext = gitHubContextService.Value.FindContextFromClipboard();
                url = clipboardContext?.Url;
            }

            var cloneDialogResult = await dialogService.Value.ShowCloneDialog(null, url);
            if (cloneDialogResult == null)
            {
                return;
            }

            url = cloneDialogResult.Url;
            var context = gitHubContextService.Value.FindContextFromUrl(url);
            if (context == null)
            {
                // Couldn't find a URL to open
                return;
            }

            var cloneUrl = gitHubContextService.Value.ToRepositoryUrl(context).ToString();
            var repositoryDir = cloneDialogResult.Path;
            var solutionDir = FindSolutionDirectory(dte.Value.Solution);
            if (solutionDir == null || !ContainsDirectory(repositoryDir, solutionDir))
            {
                await repositoryCloneService.Value.CloneOrOpenRepository(cloneUrl, repositoryDir);
            }

            await TryOpenPullRequest(context);
            gitHubContextService.Value.TryOpenFile(repositoryDir, context);
        }

        static bool ContainsDirectory(string repositoryDir, string solutionDir)
        {
            if (solutionDir.Equals(repositoryDir, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (solutionDir.StartsWith(repositoryDir + '\\', StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        static string FindSolutionDirectory(Solution solution)
        {
            var solutionPath = solution.FileName;
            if (File.Exists(solutionPath))
            {
                return Path.GetDirectoryName(solutionPath);
            }

            if (Directory.Exists(solutionPath))
            {
                return solutionPath;
            }

            return null;
        }

        async Task<bool> TryOpenPullRequest(GitHubContext context)
        {
            var pullRequest = context.PullRequest;
            if (pullRequest == null)
            {
                return false;
            }

            var host = await gitHubToolWindowManager.Value.ShowGitHubPane();
            await host.ShowPullRequest(context.Owner, context.RepositoryName, pullRequest.Value);
            return true;
        }
    }
}
