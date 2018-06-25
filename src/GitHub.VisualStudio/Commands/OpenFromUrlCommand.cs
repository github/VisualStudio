using System;
using System.IO;
using System.Windows;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using GitHub.Commands;
using GitHub.Services;
using GitHub.Primitives;
using GitHub.Services.Vssdk.Commands;
using EnvDTE;

namespace GitHub.VisualStudio.Commands
{
    [Export(typeof(IOpenFromUrlCommand))]
    public class OpenFromUrlCommand : VsCommand<string>, IOpenFromUrlCommand
    {
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
            Lazy<IRepositoryCloneService> repositoryCloneService,
            [Import(typeof(Microsoft.VisualStudio.Shell.SVsServiceProvider))] IServiceProvider sp) :
            base(CommandSet, CommandId)
        {
            this.repositoryCloneService = repositoryCloneService;
            dte = new Lazy<DTE>(() => (DTE)sp.GetService(typeof(DTE)));
            gitHubToolWindowManager = new Lazy<IGitHubToolWindowManager>(
                () => (IGitHubToolWindowManager)sp.GetService(typeof(IGitHubToolWindowManager)));

            // See https://code.msdn.microsoft.com/windowsdesktop/AllowParams-2005-9442298f
            ParametersDescription = "u";    // accept a single url
        }

        public override async Task Execute(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                url = Clipboard.GetText(TextDataFormat.Text);
            }

            var gitHubUrl = new UriString(url);
            if (!gitHubUrl.IsValidUri || !gitHubUrl.IsHypertextTransferProtocol)
            {
                return;
            }

            // Keep repos in unique dir while testing
            var defaultSubPath = "GitHubCache";

            var cloneUrl = gitHubUrl.ToRepositoryUrl().ToString();
            var targetDir = Path.Combine(repositoryCloneService.Value.DefaultClonePath, defaultSubPath, gitHubUrl.Owner);
            var repositoryDirName = gitHubUrl.RepositoryName;
            var repositoryDir = Path.Combine(targetDir, repositoryDirName);

            if (!Directory.Exists(repositoryDir))
            {
                await repositoryCloneService.Value.CloneRepository(cloneUrl, repositoryDirName, targetDir);
            }

            var solutionDir = FindSolutionDirectory(dte.Value.Solution);
            if (solutionDir == null || !ContainsDirectory(repositoryDir, solutionDir))
            {
                // Open if current solution isn't in repository directory
                dte.Value.ExecuteCommand("File.OpenFolder", repositoryDir);
                dte.Value.ExecuteCommand("View.TfsTeamExplorer");
            }

            await TryOpenPullRequest(gitHubUrl);
            TryOpenFile(gitHubUrl, repositoryDir);
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

        bool TryOpenFile(UriString gitHubUrl, string repositoryDir)
        {
            var path = FindSubPath(gitHubUrl, "/blob/master/");
            if (path == null)
            {
                return false;
            }

            var windowsPath = path.Replace('/', '\\');
            var fullPath = Path.Combine(repositoryDir, windowsPath);
            if (!File.Exists(fullPath))
            {
                return false;
            }

            dte.Value.ItemOperations.OpenFile(fullPath);
            return true;
        }

        async Task<bool> TryOpenPullRequest(UriString gitHubUrl)
        {
            var pullRequest = FindSubPath(gitHubUrl, "/pull/");
            if (pullRequest == null)
            {
                return false;
            }

            if (!int.TryParse(pullRequest, out int number))
            {
                return false;
            }

            var host = await gitHubToolWindowManager.Value.ShowGitHubPane();
            await host.ShowPullRequest(gitHubUrl.Owner, gitHubUrl.RepositoryName, number);
            return true;
        }

        static string FindSubPath(UriString gitHubUrl, string matchPath)
        {
            var url = gitHubUrl.ToString();
            var prefix = gitHubUrl.ToRepositoryUrl() + matchPath;
            if (!url.StartsWith(prefix))
            {
                return null;
            }

            var endIndex = url.IndexOf('#');
            if (endIndex == -1)
            {
                endIndex = gitHubUrl.Length;
            }

            var path = url.Substring(prefix.Length, endIndex - prefix.Length);
            return path;
        }
    }
}
