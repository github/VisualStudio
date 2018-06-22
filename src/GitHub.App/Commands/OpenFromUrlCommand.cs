using System;
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

        public override async Task Execute(string cloneUrl)
        {
            var repository = new UrlRepositoryModel(cloneUrl);
            var path = await dialogService.Value.ShowReCloneDialog(repository);
            if (path == null)
            {
                return;
            }

            var repoDirName = repository.Name;
            await repositoryCloneService.Value.CloneRepository(cloneUrl, repoDirName, path);

            dte.Value.ExecuteCommand("File.OpenFolder", path);
            dte.Value.ExecuteCommand("View.TfsTeamExplorer");
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
