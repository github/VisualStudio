namespace GitHub.ViewModels
{
    /// <summary>
    /// A file node in a pull request changes tree.
    /// </summary>
    public class PullRequestFileViewModel : IPullRequestFileViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestFileViewModel"/> class.
        /// </summary>
        /// <param name="path">The path to the file, relative to the repository.</param>
        /// <param name="changeType">The way the file was changed.</param>
        public PullRequestFileViewModel(string path, FileChangeType changeType)
        {
            ChangeType = changeType;
            FileName = System.IO.Path.GetFileName(path);
            Path = path;
        }

        /// <summary>
        /// Gets the type of change that was made to the file.
        /// </summary>
        public FileChangeType ChangeType { get; }

        /// <summary>
        /// Gets the name of the file without path information.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the path to the file, relative to the root of the repository.
        /// </summary>
        public string Path { get; }
    }
}
