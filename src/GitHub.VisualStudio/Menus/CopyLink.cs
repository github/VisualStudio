using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using GitHub.Extensions;
using GitHub.Services;
using GitHub.VisualStudio.UI;
using NullGuard;
using GitHub.Api;

namespace GitHub.VisualStudio.Menus
{
    [Export(typeof(IDynamicMenuHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CopyLink : LinkMenuBase, IDynamicMenuHandler
    {
        [ImportingConstructor]
        public CopyLink([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider, ISimpleApiClientFactory apiFactory)
            : base(serviceProvider, apiFactory)
        {
        }

        public Guid Guid => GuidList.guidContextMenuSet;
        public int CmdId => PkgCmdIDList.copyLinkCommand;

        public async void Activate([AllowNull]object data = null)
        {
            var isgithub = await IsGitHubRepo();
            if (!isgithub)
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
            var githubRepoCheckTask = IsGitHubRepo();
            return githubRepoCheckTask.Wait(250) ? githubRepoCheckTask.Result : false;
        }

    }
}
