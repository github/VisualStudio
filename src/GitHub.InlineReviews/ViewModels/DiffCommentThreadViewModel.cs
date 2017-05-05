using System;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    class DiffCommentThreadViewModel : ReactiveObject, IDiffCommentThreadViewModel
    {
        public DiffCommentThreadViewModel(
            string diffHunk,
            string path,
            InlineCommentThreadViewModel comments)
        {
            DiffHunk = diffHunk;
            Path = path;
            Comments = comments;
        }

        public string DiffHunk { get; }
        public string Path { get; }
        public ICommentThreadViewModel Comments { get; }
    }
}
