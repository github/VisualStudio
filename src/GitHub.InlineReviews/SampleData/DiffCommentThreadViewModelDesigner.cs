using System;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.SampleData
{
    class DiffCommentThreadViewModelDesigner : IDiffCommentThreadViewModel
    {
        public string DiffHunk { get; set; }
        public string Path { get; set; }
        public ICommentThreadViewModel Comments { get; set; }
    }
}
