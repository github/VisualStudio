using System;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using GitHub.Commands;
using GitHub.Services;
using GitHub.Extensions;
using GitHub.VisualStudio;
using GitHub.InlineReviews.Margins;
using GitHub.Services.Vssdk.Commands;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;

namespace GitHub.InlineReviews.Commands
{
    [Export(typeof(IToggleInlineCommentMarginCommand))]
    public class ToggleInlineCommentMarginCommand : VsCommand, IToggleInlineCommentMarginCommand
    {
        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.CommandSetGuid;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.ToggleInlineCommentMarginId;

        readonly Lazy<IVsTextManager> textManager;
        readonly Lazy<IVsEditorAdaptersFactoryService> editorAdapter;
        readonly Lazy<IUsageTracker> usageTracker;

        [ImportingConstructor]
        public ToggleInlineCommentMarginCommand(
            IGitHubServiceProvider serviceProvider,
            Lazy<IVsEditorAdaptersFactoryService> editorAdapter,
            Lazy<IUsageTracker> usageTracker) : base(CommandSet, CommandId)
        {
            textManager = new Lazy<IVsTextManager>(() => serviceProvider.GetService<SVsTextManager, IVsTextManager>());
            this.editorAdapter = editorAdapter;
            this.usageTracker = usageTracker;
        }

        public override Task Execute()
        {
            usageTracker.Value.IncrementCounter(x => x.ExecuteToggleInlineCommentMarginCommand).Forget();

            IVsTextView activeView = null;
            if (textManager.Value.GetActiveView(1, null, out activeView) == VSConstants.S_OK)
            {
                var wpfTextView = editorAdapter.Value.GetWpfTextView(activeView);
                var options = wpfTextView.Options;
                var enabled = options.GetOptionValue(InlineCommentTextViewOptions.MarginEnabledId);
                options.SetOptionValue(InlineCommentTextViewOptions.MarginEnabledId, !enabled);
            }

            return Task.CompletedTask;
        }
    }
}
