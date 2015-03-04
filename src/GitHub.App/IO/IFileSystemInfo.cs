namespace GitHub.IO
{
    public interface IFileSystemInfo
    {
        string Name { get; }
        string FullName { get; }
        bool Exists { get; }
    }
}
