using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using GitHub.Extensions;
using GitHub.Services;
using GitHub.VisualStudio.UI;

namespace GitHub.VisualStudio.Menus
{
    [Export(typeof(IDynamicMenuHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CopyLink : LinkMenuBase, IDynamicMenuHandler
    {
        [ImportingConstructor]
        public CopyLink([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public Guid Guid => GuidList.guidContextMenuSet;
        public int CmdId => PkgCmdIDList.copyLinkCommand;

        public void Activate()
        {
            if (!IsGitHubRepo())
                return;

            var link = GenerateLink();
            if (link == null)
                return;
            try
            {
                Clipboard.SetText(link);
                var ns = ServiceProvider.GetExportedValue<IStatusBarNotificationService>();
                ns?.ShowMessage(Resources.LinkCopiedToClipboardMessage);
            }
            catch
            {
                var ns = ServiceProvider.GetExportedValue<IStatusBarNotificationService>();
                ns?.ShowMessage(Resources.Error_FailedToCopyToClipboard);
            }
        }

        public bool CanShow()
        {
            return IsGitHubRepo();
        }
    }
}
