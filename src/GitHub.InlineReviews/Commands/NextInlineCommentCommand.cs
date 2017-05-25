using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using GitHub.InlineReviews.Services;
using Microsoft.VisualStudio.Text.Tagging;

namespace GitHub.InlineReviews.Commands
{
    [Export(typeof(ICommand))]
    class NextInlineCommentCommand : InlineCommentNavigationCommand
    {
        public static readonly Guid CommandSet = GlobalCommands.CommandSetGuid;
        public const int CommandId = GlobalCommands.NextInlineCommentId;

        [ImportingConstructor]
        public NextInlineCommentCommand(
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
                var next = tags.FirstOrDefault(x => x.Point > cursorPoint) ?? tags.First();
                ShowPeekComments(next.Tag);
            }

            return Task.CompletedTask;
        }
    }
}
