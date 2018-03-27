using System;
using System.Threading.Tasks;
using GitHub.VisualStudio;
using GitHub.Services.Vssdk.Commands;
using System.ComponentModel.Composition;

namespace GitHub.InlineReviews.Commands
{
    [Export(typeof(IEnableInlineCommentsCommand))]
    public class EnableInlineCommentsCommand : VsCommand, IEnableInlineCommentsCommand
    {
        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.CommandSetGuid;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.EnableInlineCommentsId;

        public EnableInlineCommentsCommand() : base(CommandSet, CommandId)
        {
        }

        public override Task Execute()
        {
            System.Diagnostics.Trace.WriteLine("EnableInlineComments");
            return Task.CompletedTask;
        }
    }
}
