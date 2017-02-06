using GitHub.Exports;
using GitHub.Services;
using GitHub.VisualStudio.UI;
using NullGuard;
using System;
using System.Windows;

namespace GitHub.VisualStudio.Menus
{
    public class CopyLink : LinkMenuBase, IDynamicMenuHandler
    {

        public CopyLink(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public Guid Guid => GuidList.guidContextMenuSet;
        public int CmdId => PkgCmdIDList.copyLinkCommand;

        public async void Activate([AllowNull]object data = null)
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
                await UsageTracker.IncrementLinkToGitHubCount();
            }
            catch
            {
                var ns = ServiceProvider.TryGetService<IStatusBarNotificationService>();
                ns?.ShowMessage(Resources.Error_FailedToCopyToClipboard);
            }
        }

        public bool CanShow()
        {
            var githubRepoCheckTask = IsCurrentFileInGitHubRepository();
            return githubRepoCheckTask.Wait(250) ? githubRepoCheckTask.Result : false;
        }
    }
}
