namespace GitHub.InlineReviews.ViewModels
{
    interface IDiffCommentThreadViewModel
    {
        string DiffHunk { get; }
        int LineNumber { get; }
        string Path { get; }
        ICommentThreadViewModel Comments { get; }
    }
}