using System;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using GitHub.Commands;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using GitHub.Services.Vssdk.Commands;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using SVsServiceProvider = Microsoft.VisualStudio.Shell.SVsServiceProvider;
using GitHub.VisualStudio.UI;

namespace GitHub.VisualStudio.Commands
{
    [Export(typeof(IOpenFromUrlCommand))]
    public class OpenFromUrlCommand : VsCommand<string>, IOpenFromUrlCommand
    {
        readonly Lazy<IGitHubContextService> gitHubContextService;
        readonly Lazy<IRepositoryCloneService> repositoryCloneService;
        readonly Lazy<IPullRequestEditorService> pullRequestEditorService;
        readonly Lazy<ITeamExplorerContext> teamExplorerContext;
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
            Lazy<IGitHubContextService> gitHubContextService,
            Lazy<IRepositoryCloneService> repositoryCloneService,
            Lazy<IPullRequestEditorService> pullRequestEditorService,
            Lazy<ITeamExplorerContext> teamExplorerContext,
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider) :
            base(CommandSet, CommandId)
        {
            this.gitHubContextService = gitHubContextService;
            this.repositoryCloneService = repositoryCloneService;
            this.pullRequestEditorService = pullRequestEditorService;
            this.teamExplorerContext = teamExplorerContext;
            this.serviceProvider = serviceProvider;
            dte = new Lazy<DTE>(() => (DTE)serviceProvider.GetService(typeof(DTE)));
            gitHubToolWindowManager = new Lazy<IGitHubToolWindowManager>(
                () => (IGitHubToolWindowManager)serviceProvider.GetService(typeof(IGitHubToolWindowManager)));

            // See https://code.msdn.microsoft.com/windowsdesktop/AllowParams-2005-9442298f
            ParametersDescription = "u";    // accept a single url
        }

        public override async Task Execute(string url)
        {
            var context = string.IsNullOrEmpty(url) ? null : gitHubContextService.Value.FindContextFromUrl(url);
            if (context == null)
            {
                context = gitHubContextService.Value.FindContextFromClipboard();
            }

            if (context == null)
            {
                // Couldn't find a URL to open
                return;
            }

            var activeDir = teamExplorerContext.Value.ActiveRepository?.LocalPath;
            if (activeDir != null)
            {
                // Try opening file in current context
                if (gitHubContextService.Value.TryOpenFile(activeDir, context))
                {
                    return;
                }
            }

            // Keep repos in unique dir while testing
            var defaultSubPath = "GitHubCache";

            var cloneUrl = gitHubContextService.Value.ToRepositoryUrl(context).ToString();
            var targetDir = Path.Combine(repositoryCloneService.Value.DefaultClonePath, defaultSubPath, context.Owner);
            var repositoryDirName = context.RepositoryName;
            var repositoryDir = Path.Combine(targetDir, repositoryDirName);

            if (!Directory.Exists(repositoryDir))
            {
                var result = ShowInfoMessage($"Clone {cloneUrl} to '{repositoryDir}'?");
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
                var result = ShowInfoMessage(string.Format(Resources.OpenRepositoryAtDir, repositoryDir));
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
            gitHubContextService.Value.TryOpenFile(repositoryDir, context);
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
