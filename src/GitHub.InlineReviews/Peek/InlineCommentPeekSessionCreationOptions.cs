using System;
using GitHub.InlineReviews.ViewModels;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Peek
{
    class InlineCommentPeekSessionCreationOptions : PeekSessionCreationOptions
    {
        public InlineCommentPeekSessionCreationOptions(
            ITextView textView,
            ITrackingPoint triggerPoint,
            CommentThreadViewModel viewModel)
            : base(textView, InlineCommentPeekRelationship.Instance.Name, triggerPoint)
        {
            ViewModel = viewModel;
        }

        public CommentThreadViewModel ViewModel { get; }
    }
}
