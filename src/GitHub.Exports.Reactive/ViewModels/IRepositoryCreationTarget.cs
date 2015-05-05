namespace GitHub.ViewModels
{
    public interface IRepositoryCreationTarget
    {
        /// <summary>
        /// The path where the repository is created. A folder named after the repository is created in this directory.
        /// </summary>
        string BaseRepositoryPath { get; set; }
    }
}
