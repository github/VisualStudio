using GitHub.VisualStudio.UI;
using NullGuard;
using System;
using GitHub.Services;

namespace GitHub.VisualStudio.Menus
{
    public class ShowGitHubPane: MenuBase, IMenuHandler
    {
        public ShowGitHubPane(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public Guid Guid => GuidList.guidGitHubCmdSet;
        public int CmdId => PkgCmdIDList.showGitHubPaneCommand;

        public void Activate([AllowNull]object data = null)
        {
            ServiceProvider.TryGetService<IGitHubToolWindowManager>()?.ShowHomePane();
        }
    }
}
