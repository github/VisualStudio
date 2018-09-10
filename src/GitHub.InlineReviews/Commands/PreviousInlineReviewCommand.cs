using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Tagging;
using GitHub.VisualStudio;
using GitHub.InlineReviews.Services;
using GitHub.Commands;
using GitHub.Services;

namespace GitHub.InlineReviews.Commands
{
    /// <summary>
    /// Navigates to and opens the the previous inline review in the currently active text view.
    /// </summary>
    [Export(typeof(IPreviousInlineCommentCommand))]
    class PreviousInlineReviewCommand : InlineReviewNavigationCommand, IPreviousInlineCommentCommand
    {
        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = Guids.CommandSetGuid;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = PkgCmdIDList.PreviousInlineCommentId;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreviousInlineReviewCommand"/> class.
        /// </summary>
        /// <param name="serviceProvider">The GitHub service provider.</param>
        /// <param name="tagAggregatorFactory">The tag aggregator factory.</param>
        /// <param name="peekService">The peek service.</param>
        [ImportingConstructor]
        protected PreviousInlineReviewCommand(
            IGitHubServiceProvider serviceProvider,
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            IInlineReviewPeekService peekService)
            : base(serviceProvider, tagAggregatorFactory, peekService, CommandSet, CommandId)
        {
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>A task that tracks the execution of the command.</returns>
        public override Task Execute(InlineReviewNavigationParams parameter)
        {
            var textViews = GetCurrentTextViews().ToList();
            var tags = GetTags(textViews);

            if (tags.Count > 0)
            {
                var cursorPoint = GetCursorPoint(textViews[0], parameter);
                var next = tags.LastOrDefault(x => x.Point < cursorPoint) ?? tags.Last();
                ShowPeekComments(parameter, next.TextView, next.Tag, textViews);
            }

            return Task.CompletedTask;
        }
    }
}
