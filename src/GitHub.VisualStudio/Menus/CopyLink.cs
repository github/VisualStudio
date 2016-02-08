using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using GitHub.Extensions;
using GitHub.Services;

namespace GitHub.VisualStudio.Menus
{
    [Export(typeof(IDynamicMenuHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CopyLink: LinkMenuBase, IDynamicMenuHandler
    {
        [ImportingConstructor]
        public CopyLink([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public Guid Guid { get { return GuidList.guidContextMenuSet; } }
        public int CmdId { get { return PkgCmdIDList.copyLinkCommand; } }

        public void Activate()
        {
            if (!IsGitHubRepo())
                return;

            var link = GenerateLink();
            if (link == null)
                return;
            Clipboard.SetText(link.AbsoluteUri);
            var ns = ServiceProvider.GetExportedValue<IStatusBarNotificationService>();
            ns?.ShowMessage(Resources.LinkCopiedToClipboardMessage);
        }

        public bool CanShow()
        {
            return IsGitHubRepo();
        }
    }
}
