using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.Tags;
using Microsoft.VisualStudio.Text.Tagging;
using ReactiveUI;

namespace GitHub.InlineReviews.Commands
{
    /// <summary>
    /// Navigates to and opens the the next inline comment thread in the currently active text view.
    /// </summary>
    /// <remarks>
    /// This command is not visible in the UI - it's used by the PR details pane to show the first
    /// comment thread when the "# of comments" hyperlink is clicked.
    /// </remarks>
    [ExportCommand(typeof(InlineReviewsPackage))]
    class FirstInlineCommentCommand : InlineCommentNavigationCommand
    {
        /// <summary>
        /// Gets the GUID of the group the command belongs to.
        /// </summary>
        public static readonly Guid CommandSet = GlobalCommands.CommandSetGuid;

        /// <summary>
        /// Gets the numeric identifier of the command.
        /// </summary>
        public const int CommandId = GlobalCommands.FirstInlineCommentId;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirstInlineCommentCommand"/> class.
        /// </summary>
        /// <param name="tagAggregatorFactory">The tag aggregator factory.</param>
        /// <param name="peekService">The peek service.</param>
        [ImportingConstructor]
        public FirstInlineCommentCommand(
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            IInlineCommentPeekService peekService)
            : base(tagAggregatorFactory, peekService, CommandSet, CommandId)
        {
        }

        protected override bool IsEnabled => true;

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>A task that tracks the execution of the command.</returns>
        protected override Task Execute()
        {
            if (!TryShow())
            {
                var textView = GetCurrentTextView();
                var aggregator = CreateTagAggregator(textView);
                aggregator.BatchedTagsChanged += TagsChanged;
            }

            return Task.CompletedTask;
        }

        bool TryShow()
        {
            var textView = GetCurrentTextView();
            var tags = GetTags(textView);

            if (tags.Count > 0)
            {
                ShowPeekComments(textView, tags.First().Tag);
                return true;
            }

            return false;
        }

        void TagsChanged(object sender, BatchedTagsChangedEventArgs e)
        {
            // The peek view doesn't show unless we delay trying to show it.
            var timer = new DispatcherTimer(DispatcherPriority.Background);
            timer.Interval = TimeSpan.FromMilliseconds(1500);
            timer.Tick += (_, __) =>
            {
                TryShow();
                timer.Stop();
            };
            timer.Start();

            var aggregator = (ITagAggregator<InlineCommentTag>)sender;
            aggregator.BatchedTagsChanged -= TagsChanged;
        }
    }
}
