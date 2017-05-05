namespace GitHub.InlineReviews.ViewModels
{
    interface IDiffCommentThreadViewModel
    {
        string DiffHunk { get; }
        string Path { get; }
        ICommentThreadViewModel Comments { get; }
    }
}