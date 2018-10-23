using System.Reactive;
using System.Windows.Input;
using GitHub.Validation;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
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

        /// <summary>
        /// Command that launches a dialog to browse for the directory in which to create the repository.
        /// </summary>
        ReactiveCommand<Unit, Unit> BrowseForDirectory { get; }
    }
}
