using System;
using System.IO;
using System.Windows;
using System.ComponentModel.Composition;
using GitHub.Services;
using GitHub.Primitives;
using GitHub.VisualStudio;
using GitHub.Services.Vssdk.Commands;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Commands
{
    [Export(typeof(IOpenFromUrlCommand))]
    public class OpenFromUrlCommand : VsCommand<string>, IOpenFromUrlCommand
    {
        readonly Lazy<IRepositoryCloneService> repositoryCloneService;
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
            [Import(typeof(SVsServiceProvider))] IServiceProvider sp) :
            base(CommandSet, CommandId)
        {
            this.repositoryCloneService = repositoryCloneService;
            dte = new Lazy<DTE>(() => (DTE)sp.GetService(typeof(DTE)));

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
            if (solutionDir == null || !repositoryDir.StartsWith(solutionDir + '\\', StringComparison.OrdinalIgnoreCase))
            {
                // Open if current solution isn't in repository directory
                dte.Value.ExecuteCommand("File.OpenFolder", repositoryDir);
                dte.Value.ExecuteCommand("View.TfsTeamExplorer");
            }

            TryOpenFile(url, repositoryDir);
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

        bool TryOpenFile(string url, string repositoryDir)
        {
            var path = FindPath(url);
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

        static string FindPath(string cloneUrl, string matchPath = "/blob/master/")
        {
            var uriString = new UriString(cloneUrl);
            var prefix = uriString.ToRepositoryUrl() + matchPath;
            if (!cloneUrl.StartsWith(prefix))
            {
                return null;
            }

            var endIndex = cloneUrl.IndexOf('#');
            if (endIndex == -1)
            {
                endIndex = cloneUrl.Length;
            }

            var path = cloneUrl.Substring(prefix.Length, endIndex - prefix.Length);
            return path;
        }
    }
}
