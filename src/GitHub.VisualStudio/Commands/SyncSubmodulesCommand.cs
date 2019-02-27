using System;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using GitHub.Logging;
using GitHub.Commands;
using GitHub.Services;
using GitHub.Services.Vssdk.Commands;
using Serilog;
using System.IO;
using System.Globalization;
using System.Linq;

namespace GitHub.VisualStudio.Commands
{
    /// <summary>
    /// Command to sync submodules in local repository.
    /// </summary>
    [Export(typeof(ISyncSubmodulesCommand))]
    public class SyncSubmodulesCommand : VsCommand, ISyncSubmodulesCommand
    {
        static readonly ILogger log = LogManager.ForContext<SyncSubmodulesCommand>();

        readonly Lazy<IPullRequestService> lazyPullRequestService;
        readonly Lazy<IStatusBarNotificationService> lazyStatusBarNotificationService;
        readonly Lazy<IVSGitExt> lazyVSGitExt;

        [ImportingConstructor]
        protected SyncSubmodulesCommand(
            Lazy<IPullRequestService> pullRequestService,
            Lazy<IStatusBarNotificationService> statusBarNotificationService,
            Lazy<IVSGitExt> gitExt)
            : base(CommandSet, CommandId)
        {
            lazyPullRequestService = pullRequestService;
            lazyStatusBarNotificationService = statusBarNotificationService;
            lazyVSGitExt = gitExt;
        }

        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.guidGitHubCmdSet;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.syncSubmodulesCommand;

        /// <summary>
        /// Syncs submodules.
        /// </summary>
        public override async Task Execute()
        {
            try
            {
                var complete = await SyncSubmodules();
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error syncing submodules");
                lazyStatusBarNotificationService.Value.ShowMessage("Error syncing submodules");
            }
        }

        /// <summary>
        /// Sync submodules in local repository.
        /// </summary>
        /// <returns>Tuple with bool that is true if command completed successfully and string with
        /// output from sync submodules Git command.</returns>
        public async Task<Tuple<bool, string>> SyncSubmodules()
        {
            var pullRequestService = lazyPullRequestService.Value;
            var statusBarNotificationService = lazyStatusBarNotificationService.Value;
            var gitExt = lazyVSGitExt.Value;

            var repository = gitExt.ActiveRepositories.FirstOrDefault();
            if (repository == null)
            {
                statusBarNotificationService.ShowMessage("No local Git repository");
                return new Tuple<bool, string>(true, "No local Git repository");
            }

            var writer = new StringWriter(CultureInfo.CurrentCulture);
            var complete = await pullRequestService.SyncSubmodules(repository, line =>
            {
                writer.WriteLine(line);
                statusBarNotificationService.ShowMessage(line);
            });

            if (!complete)
            {
                statusBarNotificationService.ShowMessage("Failed to sync submodules." + Environment.NewLine + writer);
                return new Tuple<bool, string>(false, writer.ToString());
            }

            statusBarNotificationService.ShowMessage(string.Empty);
            return new Tuple<bool, string>(true, writer.ToString());
        }
    }
}
