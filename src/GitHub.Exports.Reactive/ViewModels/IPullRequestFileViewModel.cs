namespace GitHub.ViewModels
{
    public interface IPullRequestFileViewModel : IPullRequestChangeNode
    {
        bool Added { get; }
        bool Deleted { get; }
        string FileName { get; }
    }
}