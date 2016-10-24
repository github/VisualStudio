using GitHub.Models;

namespace GitHub.ViewModels
{
    public interface IPullRequestFileNode : IPullRequestChangeNode
    {
        string FileName { get; }
        PullRequestFileStatus Status { get; }
    }
}