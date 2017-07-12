using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using GitHub.InlineReviews.Services;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace GitHub.InlineReviews.Commands
{
    /// <summary>
    /// Navigates to and opens the the next inline comment thread in the currently active text view.
    /// </summary>
    [ExportCommand(typeof(InlineReviewsPackage))]
    [Export(typeof(INextInlineCommentCommand))]
    class NextInlineCommentCommand : InlineCommentNavigationCommand, INextInlineCommentCommand
    {
        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = GlobalCommands.CommandSetGuid;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = GlobalCommands.NextInlineCommentId;

        /// <summary>
        /// Initializes a new instance of the <see cref="NextInlineCommentCommand"/> class.
        /// </summary>
        /// <param name="tagAggregatorFactory">The tag aggregator factory.</param>
        /// <param name="peekService">The peek service.</param>
        [ImportingConstructor]
        public NextInlineCommentCommand(
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            IInlineCommentPeekService peekService)
            : base(tagAggregatorFactory, peekService, CommandSet, CommandId)
        {
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>A task that tracks the execution of the command.</returns>
        public override Task Execute(InlineCommentNavigationParams parameter)
        {
            var textViews = GetCurrentTextViews().ToList();
            var tags = GetTags(textViews);

            if (tags.Count > 0)
            {
                var cursorPoint = GetCursorPoint(textViews[0], parameter);
                var next = tags.FirstOrDefault(x => x.Point > cursorPoint) ?? tags.First();
                ShowPeekComments(parameter, next.TextView, next.Tag, textViews);
            }

            return Task.CompletedTask;
        }
    }
}
