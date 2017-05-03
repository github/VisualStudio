using System;
using System.Collections.Generic;
using GitHub.InlineReviews.Models;
using GitHub.Services;
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
            IPullRequestReviewSession session,
            IReadOnlyList<InlineCommentModel> comments)
            : base(textView, InlineCommentPeekRelationship.Instance.Name, triggerPoint)
        {
            Session = session;
            Comments = comments;
        }

        public IPullRequestReviewSession Session { get; }
        public IReadOnlyList<InlineCommentModel> Comments { get; }
    }
}
