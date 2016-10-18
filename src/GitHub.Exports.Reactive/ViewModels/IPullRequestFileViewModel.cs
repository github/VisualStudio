using GitHub.Models;

namespace GitHub.ViewModels
{
    public interface IPullRequestFileViewModel : IPullRequestChangeNode
    {
        string FileName { get; }
        PullRequestFileStatus Status { get; }
    }
}