using GitHub.VisualStudio.UI;
using System;
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio
{
    [Export(typeof(IMenuHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShowGitHubPane: MenuBase, IMenuHandler
    {
        public Guid Guid { get { return GuidList.guidGitHubCmdSet; } }
        public int CmdId { get { return PkgCmdIDList.showGitHubPaneCommand; } }

        public void Activate()
        {
            GitHubPane.Activate();
        }
    }
}
