using System;
using System.ComponentModel.Composition;
using GitHub.Commands;
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
            Lazy<IRepositoryCloneService> repositoryCloneService) :
            base(CommandSet, CommandId)
        {
            this.dialogService = dialogService;
            this.repositoryCloneService = repositoryCloneService;

            // See https://code.msdn.microsoft.com/windowsdesktop/AllowParams-2005-9442298f
            ParametersDescription = "u";    // accept a single url
        }

        public override async Task Execute(string url)
        {
            var cloneDialogResult = await dialogService.Value.ShowCloneDialog(null, url);
            if (cloneDialogResult != null)
            {
                await repositoryCloneService.Value.CloneOrOpenRepository(cloneDialogResult);
            }
        }
    }
}
