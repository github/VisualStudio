using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using GitHub.Commands;
using GitHub.Exports;
using GitHub.Services;
using GitHub.VisualStudio.UI;

namespace GitHub.VisualStudio.Commands
{
    /// <summary>
    /// Copies a link to the clipboard of the currently selected text on GitHub.com or an
    /// Enterprise instance.
    /// </summary>
    [Export(typeof(ICopyLinkCommand))]
    public class CopyLinkCommand : LinkCommandBase, ICopyLinkCommand
    {
        [ImportingConstructor]
        protected CopyLinkCommand(IGitHubServiceProvider serviceProvider, Lazy<IGitService> gitService)
            : base(CommandSet, CommandId, serviceProvider, gitService)
        {
        }

        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.guidContextMenuSet;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.copyLinkCommand;

        /// <summary>
        /// Copies the link to the clipboard.
        /// </summary>
        public async override Task Execute()
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
