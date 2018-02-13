using System;
using GitHub.Extensions;
using GitHub.Services;

namespace GitHub.VisualStudio.Menus
{
    public class ShowGitHubPane : MenuBase, IMenuHandler
    {
        public ShowGitHubPane(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
        }

        public Guid Guid => Guids.guidGitHubCmdSet;
        public int CmdId => PkgCmdIDList.showGitHubPaneCommand;

        public void Activate(object data = null)
        {
            ServiceProvider.TryGetService<IGitHubToolWindowManager>()?.ShowGitHubPane();
        }
    }
}
