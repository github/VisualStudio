using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using GitHub.InlineReviews.Services;
using Microsoft.VisualStudio.Text.Tagging;

namespace GitHub.InlineReviews.Commands
{
    /// <summary>
    /// Navigates to and opens the the previous inline comment thread in the currently active text view.
    /// </summary>
    [ExportCommand(typeof(InlineReviewsPackage))]
    class PreviousInlineCommentCommand : InlineCommentNavigationCommand
    {
        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = GlobalCommands.CommandSetGuid;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = GlobalCommands.PreviousInlineCommentId;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreviousInlineCommentCommand"/> class.
        /// </summary>
        /// <param name="tagAggregatorFactory">The tag aggregator factory.</param>
        /// <param name="peekService">The peek service.</param>
        [ImportingConstructor]
        public PreviousInlineCommentCommand(
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            IInlineCommentPeekService peekService)
            : base(tagAggregatorFactory, peekService, CommandSet, CommandId)
        {
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>A task that tracks the execution of the command.</returns>
        protected override Task Execute()
        {
            var textView = GetCurrentTextView();
            var tags = GetTags(textView);

            if (tags.Count > 0)
            {
                var cursorPoint = textView.Caret.Position.BufferPosition.Position;
                var next = tags.LastOrDefault(x => x.Point < cursorPoint) ?? tags.Last();
                ShowPeekComments(textView, next.Tag);
            }

            return Task.CompletedTask;
        }
    }
}
