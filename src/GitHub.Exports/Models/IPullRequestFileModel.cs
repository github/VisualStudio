namespace GitHub.Models
{
    public enum PullRequestFileStatus
    {
        Modified,
        Added,
        Removed,
    }

    public interface IPullRequestFileModel
    {
        string FileName { get; }
        PullRequestFileStatus Status { get; }
    }
}