using GitHub.Extensions;
using GitHub.Services;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace GitHub.VisualStudio.Menus
{
    [Export(typeof(IDynamicMenuHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class OpenLink: LinkMenuBase, IDynamicMenuHandler
    {
        [ImportingConstructor]
        public OpenLink([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public Guid Guid { get { return GuidList.guidContextMenuSet; } }
        public int CmdId { get { return PkgCmdIDList.openLinkCommand; } }

        public void Activate()
        {
            if (!IsGitHubRepo())
                return;

            var link = GenerateLink();
            if (link == null)
                return;
            var browser = ServiceProvider.GetExportedValue<IVisualStudioBrowser>();
            browser?.OpenUrl(link);
        }

        public bool CanShow()
        {
            return IsGitHubRepo();
        }
    }
}
