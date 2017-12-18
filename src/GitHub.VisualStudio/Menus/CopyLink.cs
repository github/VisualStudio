using GitHub.Exports;
using GitHub.Services;
using GitHub.VisualStudio.UI;
using System;
using System.Windows;
using GitHub.Extensions;

namespace GitHub.VisualStudio.Menus
{
    public class CopyLink : LinkMenuBase, IDynamicMenuHandler
    {
        public CopyLink(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
        }

        public Guid Guid => Guids.guidContextMenuSet;
        public int CmdId => PkgCmdIDList.copyLinkCommand;

        public async void Activate(object data = null)
        {
            var isgithub = await IsGitHubRepo();
            if (!isgithub)
                return;

            var link = await GenerateLink(LinkType.Blob);
            if (link == null)
                return;
            try
            {
                Clipboard.SetText(link);
                var ns = ServiceProvider.TryGetService<IStatusBarNotificationService>();
                ns?.ShowMessage(Resources.LinkCopiedToClipboardMessage);
                await UsageTracker.IncrementCounter(x => x.NumberOfLinkToGitHub);
            }
            catch
            {
                var ns = ServiceProvider.TryGetService<IStatusBarNotificationService>();
                ns?.ShowMessage(Resources.Error_FailedToCopyToClipboard);
            }
        }
    }
}
