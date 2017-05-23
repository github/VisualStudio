using System;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    class DiffCommentThreadViewModel : ReactiveObject, IDiffCommentThreadViewModel
    {
        public DiffCommentThreadViewModel(
            string diffHunk,
            int lineNumber,
            string path,
            InlineCommentThreadViewModel comments)
        {
            DiffHunk = diffHunk;
            LineNumber = lineNumber;
            Path = path;
            Comments = comments;
        }

        public string DiffHunk { get; }
        public int LineNumber { get; }
        public string Path { get; }
        public ICommentThreadViewModel Comments { get; }
    }
}
