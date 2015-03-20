using System.Windows.Input;
using GitHub.Models;
using GitHub.Validation;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface IRepositoryCreationViewModel : IViewModel
    {
        /// <summary>
        /// The name of the repository.
        /// </summary>
        string RepositoryName { get; set; }

        /// <summary>
        /// The path where the repository is created. A folder named after the repository is created in this directory.
        /// </summary>
        string BaseRepositoryPath { get; set; }

        /// <summary>
        /// The name of the repository after characters not allowed in a git repository name are replaced with
        /// dashes.
        /// </summary>
        string SafeRepositoryName { get; }
        ReactivePropertyValidator<string> RepositoryNameValidator { get; }
        ReactivePropertyValidator<string> SafeRepositoryNameWarningValidator { get; }
        ReactivePropertyValidator<string> BaseRepositoryPathValidator { get; }
        string Description { get; set; }

        ReactiveList<IAccount> Accounts { get; }

        /// <summary>
        /// The account or organization that will be the owner of the created repository.
        /// </summary>
        IAccount SelectedAccount { get; set; }

        GitIgnoreItem SelectedGitIgnoreTemplate { get; set; }

        LicenseItem SelectedLicense { get; set; }

        /// <summary>
        /// The list of GitIgnore templates supported by repository creation
        /// </summary>
        ReactiveList<GitIgnoreItem> GitIgnoreTemplates { get; }
        ReactiveList<LicenseItem> Licenses { get; }

        /// <summary>
        /// Indicates whether the created repository should be private or not.
        /// </summary>
        bool KeepPrivate { get; set; }

        /// <summary>
        /// Indicates whether the user can create a private repository. This is false if the user is not a paid
        /// account or if the user has run out of repositories for their current plan.
        /// </summary>
        bool CanKeepPrivate { get; }
        bool ShowUpgradeToMicroPlanWarning { get; }
        bool ShowUpgradePlanWarning { get; }

        bool IsPublishing { get; }

        /// <summary>
        /// Command that launches a dialog to browse for the directory in which to create the repository.
        /// </summary>
        ICommand BrowseForDirectory { get; }

        /// <summary>
        /// Command that creates the repository.
        /// </summary>
        ICommand CreateRepository { get; }
        /// <summary>
        /// Command that opens a browser to a page for upgrading the user's plan.
        /// </summary>
        ICommand UpgradeAccountPlan { get; }
    }
}
