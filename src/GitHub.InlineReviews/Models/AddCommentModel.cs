using System;

namespace GitHub.InlineReviews.Models
{
    class AddCommentModel
    {
        public AddCommentModel(string commitSha, int diffLine, int lineNumber)
        {
            CommitSha = commitSha;
            DiffLine = diffLine;
            LineNumber = lineNumber;
        }

        public string CommitSha { get; }
        public int DiffLine { get; }
        public int LineNumber { get; }
    }
}
