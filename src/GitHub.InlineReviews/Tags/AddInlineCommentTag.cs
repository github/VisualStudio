using System;
using GitHub.Extensions;
using GitHub.Services;
using GitHub.Models;

namespace GitHub.InlineReviews.Tags
{
    class AddInlineCommentTag : InlineCommentTag
    {
        public AddInlineCommentTag(
            IPullRequestSession session,
            string commitSha,
            string filePath,
            int diffLine,
            int lineNumber,
            DiffChangeType diffChangeType)
            : base(session, lineNumber, diffChangeType)
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
