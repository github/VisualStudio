using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using GitHub.InlineReviews.Services;
using Microsoft.VisualStudio.Text.Tagging;

namespace GitHub.InlineReviews.Commands
{
    [Export(typeof(ICommand))]
    class PreviousInlineCommentCommand : InlineCommentNavigationCommand
    {
        public static readonly Guid CommandSet = GlobalCommands.CommandSetGuid;
        public const int CommandId = GlobalCommands.PreviousInlineCommentId;

        [ImportingConstructor]
        public PreviousInlineCommentCommand(
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            IInlineCommentPeekService peekService)
            : base(tagAggregatorFactory, peekService, CommandSet, CommandId)
        {
        }

        protected override Task Execute()
        {
            var textView = GetCurrentTextView();
            var tags = GetTags(textView);

            if (tags.Count > 0)
            {
                var cursorPoint = textView.Caret.Position.BufferPosition.Position;
                var next = tags.LastOrDefault(x => x.Point < cursorPoint) ?? tags.Last();
                ShowPeekComments(next.Tag);
            }

            return Task.CompletedTask;
        }
    }
}
