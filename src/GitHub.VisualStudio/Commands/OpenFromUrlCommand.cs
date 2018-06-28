using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using GitHub.Commands;
using GitHub.Services;
using GitHub.App.Services;
using GitHub.Services.Vssdk.Commands;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using SVsServiceProvider = Microsoft.VisualStudio.Shell.SVsServiceProvider;

namespace GitHub.VisualStudio.Commands
{
    [Export(typeof(IOpenFromUrlCommand))]
    public class OpenFromUrlCommand : VsCommand<string>, IOpenFromUrlCommand
    {
        readonly Lazy<GitHubContextService> gitHubContextService;
        readonly Lazy<IRepositoryCloneService> repositoryCloneService;
        readonly Lazy<IPullRequestEditorService> pullRequestEditorService;
        readonly Lazy<IGitHubToolWindowManager> gitHubToolWindowManager;
        readonly Lazy<DTE> dte;
        readonly IServiceProvider serviceProvider;

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
            Lazy<GitHubContextService> gitHubContextService,
            Lazy<IRepositoryCloneService> repositoryCloneService,
            Lazy<IPullRequestEditorService> pullRequestEditorService,
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider) :
            base(CommandSet, CommandId)
        {
            this.gitHubContextService = gitHubContextService;
            this.repositoryCloneService = repositoryCloneService;
            this.pullRequestEditorService = pullRequestEditorService;
            this.serviceProvider = serviceProvider;
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
                url = Clipboard.GetText(TextDataFormat.Text);
            }

            var context = gitHubContextService.Value.FindContextFromUrl(url);
            context = context ?? gitHubContextService.Value.FindContextFromBrowser();

            if (context == null)
            {
                return;
            }

            // Keep repos in unique dir while testing
            var defaultSubPath = "GitHubCache";

            var cloneUrl = gitHubContextService.Value.ToRepositoryUrl(context).ToString();
            var targetDir = Path.Combine(repositoryCloneService.Value.DefaultClonePath, defaultSubPath, context.Owner);
            var repositoryDirName = context.RepositoryName;
            var repositoryDir = Path.Combine(targetDir, repositoryDirName);

            if (!Directory.Exists(repositoryDir))
            {
                var result = ShowInfoMessage($"Clone '{cloneUrl}' to '{repositoryDir}'?");
                switch (result)
                {
                    case VSConstants.MessageBoxResult.IDYES:
                        await repositoryCloneService.Value.CloneRepository(cloneUrl, repositoryDirName, targetDir);
                        // Open the cloned repository
                        dte.Value.ExecuteCommand("File.OpenFolder", repositoryDir);
                        dte.Value.ExecuteCommand("View.TfsTeamExplorer");
                        break;
                    case VSConstants.MessageBoxResult.IDNO:
                        // Target the current solution
                        repositoryDir = FindSolutionDirectory(dte.Value.Solution);
                        if (repositoryDir == null)
                        {
                            // No current solution to use
                            return;
                        }

                        break;
                    case VSConstants.MessageBoxResult.IDCANCEL:
                        return;
                }
            }

            var solutionDir = FindSolutionDirectory(dte.Value.Solution);
            if (solutionDir == null || !ContainsDirectory(repositoryDir, solutionDir))
            {
                var result = ShowInfoMessage($"Open repository fiolder at '{repositoryDir}'?");
                switch (result)
                {
                    case VSConstants.MessageBoxResult.IDYES:
                        // Open if current solution isn't in repository directory
                        dte.Value.ExecuteCommand("File.OpenFolder", repositoryDir);
                        dte.Value.ExecuteCommand("View.TfsTeamExplorer");
                        break;
                    case VSConstants.MessageBoxResult.IDNO:
                        break;
                    case VSConstants.MessageBoxResult.IDCANCEL:
                        return;
                }
            }

            await TryOpenPullRequest(context);
            TryOpenFile(context, repositoryDir);
        }

        VSConstants.MessageBoxResult ShowInfoMessage(string message)
        {
            return (VSConstants.MessageBoxResult)VsShellUtilities.ShowMessageBox(serviceProvider, message, null,
                OLEMSGICON.OLEMSGICON_QUERY, OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
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

        bool TryOpenFile(GitHubContext context, string repositoryDir)
        {
            var path = context.Path;
            if (path == null)
            {
                return false;
            }

            var windowsPath = path.Replace('/', '\\');
            var fullPath = Path.Combine(repositoryDir, windowsPath);
            if (!File.Exists(fullPath))
            {
                // Search by filename only
                var fileName = Path.GetFileName(path);
                fullPath = Directory.EnumerateFiles(repositoryDir, fileName, SearchOption.AllDirectories).FirstOrDefault();
            }

            if (fullPath == null)
            {
                return false;
            }

            dte.Value.ItemOperations.OpenFile(fullPath);

            var lineNumber = context.Line;
            if (lineNumber == null)
            {
                var number = lineNumber.Value - 1;
                var activeView = pullRequestEditorService.Value.FindActiveView();
                ErrorHandler.ThrowOnFailure(activeView.SetCaretPos(number, 0));
                ErrorHandler.ThrowOnFailure(activeView.CenterLines(number, 1));
            }

            return true;
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
