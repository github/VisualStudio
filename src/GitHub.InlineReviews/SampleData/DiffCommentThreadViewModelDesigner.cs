using System;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.SampleData
{
    public class DiffCommentThreadViewModelDesigner : IDiffCommentThreadViewModel
    {
        public string DiffHunk { get; set; }
        public int LineNumber { get; set; }
        public event EventHandler Disposed = delegate { };
        public string Path { get; set; }
        public ICommentThreadViewModel Comments { get; set; }
    }
}
