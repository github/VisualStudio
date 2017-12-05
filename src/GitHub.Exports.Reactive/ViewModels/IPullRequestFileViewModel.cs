namespace GitHub.ViewModels
{
    /// <summary>
    /// Describes the way in which a file is changed in a pull request.
    /// </summary>
    public enum FileChangeType
    {
        /// <summary>
        /// The file contents were changed.
        /// </summary>
        Changed,

        /// <summary>
        /// The file was added.
        /// </summary>
        Added,

        /// <summary>
        /// The file was deleted.
        /// </summary>
        Removed,
    }

    /// <summary>
    /// Represents a file node in a pull request changes tree.
    /// </summary>
    public interface IPullRequestFileViewModel : IPullRequestChangeNode
    {
        /// <summary>
        /// Gets the type of change that was made to the file.
        /// </summary>
        FileChangeType ChangeType { get; }

        /// <summary>
        /// Gets the name of the file without path information.
        /// </summary>
        string FileName { get; }
    }
}