using GitHub.Validation;

namespace GitHub.ViewModels
{
    public interface IRepositoryCreationTarget
    {
        /// <summary>
        /// The path where the repository is created. A folder named after the repository is created in this directory.
        /// </summary>
        string BaseRepositoryPath { get; set; }

        /// <summary>
        /// Validates the base repository path.
        /// </summary>
        ReactivePropertyValidator<string> BaseRepositoryPathValidator { get; }
    }
}
