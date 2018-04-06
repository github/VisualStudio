using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Commands;
using GitHub.Logging;
using GitHub.Services;
using GitHub.Services.Vssdk.Commands;
using Serilog;

namespace GitHub.VisualStudio.Commands
{
    /// <summary>
    /// Opens the GitHub pane.
    /// </summary>
    [Export(typeof(IShowGitHubPaneCommand))]
    public class ShowGitHubPaneCommand : VsCommand, IShowGitHubPaneCommand
    {
        static readonly ILogger log = LogManager.ForContext<ShowGitHubPaneCommand>();
        readonly IGitHubServiceProvider serviceProvider;

        [ImportingConstructor]
        protected ShowGitHubPaneCommand(IGitHubServiceProvider serviceProvider)
            : base(CommandSet, CommandId)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.guidGitHubCmdSet;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.showGitHubPaneCommand;

        /// <summary>
        /// Shows the GitHub pane.
        /// </summary>
        public override async Task Execute()
        {
            try
            {
                var host = await serviceProvider.TryGetService<IGitHubToolWindowManager>().ShowGitHubPane();
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error showing opening GitHub pane");
            }
        }
    }
}
