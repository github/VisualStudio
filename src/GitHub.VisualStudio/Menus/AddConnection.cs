using GitHub.Services;
using System;

namespace GitHub.VisualStudio.Menus
{
    public class AddConnection : MenuBase, IMenuHandler
    {
        readonly Lazy<IDialogService> dialogService;

        public AddConnection(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            dialogService = new Lazy<IDialogService>(() => ServiceProvider.TryGetService<IDialogService>());
        }

        public Guid Guid => Guids.guidGitHubCmdSet;
        public int CmdId => PkgCmdIDList.addConnectionCommand;

        public void Activate(object data = null)
        {
            dialogService.Value.ShowLoginDialog();
        }
    }
}
