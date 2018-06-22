using System;
using System.IO;
using System.ComponentModel.Composition;
using GitHub.UI;
using GitHub.Models;
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
        readonly Lazy<IDialogService> dialogService;
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
            Lazy<IDialogService> dialogService,
            Lazy<IRepositoryCloneService> repositoryCloneService,
            [Import(typeof(SVsServiceProvider))] IServiceProvider sp) :
            base(CommandSet, CommandId)
        {
            this.dialogService = dialogService;
            this.repositoryCloneService = repositoryCloneService;
            dte = new Lazy<DTE>(() => (DTE)sp.GetService(typeof(DTE)));

            // See https://code.msdn.microsoft.com/windowsdesktop/AllowParams-2005-9442298f
            ParametersDescription = "u";    // accept a single url
        }

        public override async Task Execute(string url)
        {
            var repository = new UrlRepositoryModel(url);
            var targetDir = await dialogService.Value.ShowReCloneDialog(repository);
            if (targetDir == null)
            {
                return;
            }

            await repositoryCloneService.Value.CloneRepository(repository.CloneUrl, repository.Name, targetDir);
            var repositoryDir = Path.Combine(targetDir, repository.Name);
            if (Directory.Exists(repositoryDir))
            {
                return;
            }

            dte.Value.ExecuteCommand("File.OpenFolder", repositoryDir);
            dte.Value.ExecuteCommand("View.TfsTeamExplorer");

            TryOpenFile(url, repositoryDir);
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
            return cloneUrl.StartsWith(prefix) ? cloneUrl.Substring(prefix.Length) : null;
        }

        class UrlRepositoryModel : IRepositoryModel
        {
            public UrlRepositoryModel(string url)
            {
                var repositoryUrl = new UriString(url).ToRepositoryUrl();
                CloneUrl = new UriString(repositoryUrl.ToString());
            }

            public void SetIcon(bool isPrivate, bool isFork)
            {
            }

            public UriString CloneUrl { get; }
            public string Name => CloneUrl.RepositoryName;
            public string Owner => CloneUrl.Owner;
            public Octicon Icon { get; }
        }
    }
}
