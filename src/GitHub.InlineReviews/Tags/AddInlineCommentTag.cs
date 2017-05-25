using System;
using GitHub.Extensions;
using GitHub.Services;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Tags
{
    class AddInlineCommentTag : InlineCommentTag
    {
        public AddInlineCommentTag(
            IPullRequestSession session,
            ITextView textView,
            string commitSha,
            string filePath,
            int diffLine,
            int lineNumber)
            : base(session, textView, lineNumber)
        {
            Guard.ArgumentNotNull(commitSha, nameof(commitSha));

            CommitSha = commitSha;
            DiffLine = diffLine;
            FilePath = filePath;
        }

        public string CommitSha { get; }
        public int DiffLine { get; }
        public string FilePath { get; }
    }
}
