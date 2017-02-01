namespace GitHub.Models
{
    public enum PullRequestFileStatus
    {
        Modified,
        Added,
        Removed,
        Renamed,
    }

    public interface IPullRequestFileModel
    {
        string FileName { get; }
        string Sha { get; }
        PullRequestFileStatus Status { get; }
    }
}