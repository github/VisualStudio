using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using GitHub.Services;
using GitHub.UI;
using GitHub.Extensions;

namespace GitHub.VisualStudio.Menus
{
    [Export(typeof(IMenuHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AddConnection: MenuBase, IMenuHandler
    {
        [ImportingConstructor]
        public AddConnection([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public Guid Guid { get { return GuidList.guidGitHubCmdSet; } }
        public int CmdId { get { return PkgCmdIDList.addConnectionCommand; } }

        public void Activate()
        {
            StartFlow(UIControllerFlow.Authentication);
        }

        void StartFlow(UIControllerFlow controllerFlow)
        {
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            uiProvider.RunUI(controllerFlow, null);
        }
    }
}
