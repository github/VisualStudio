using System.Collections.Generic;
using System.Windows.Input;
using GitHub.Models;
using GitHub.Validation;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Represents the properties of a repository creation/publish form.
    /// </summary>
    /// <remarks>
    /// These are effectively the properties in common between the RepositoryPublishViewModel and the
    /// RepositoryCreationViewModel.
    /// </remarks>
    public interface IRepositoryForm : IViewModel
    {
        /// <summary>
        /// The name of the repository.
        /// </summary>
        string RepositoryName { get; set; }

        /// <summary>
        /// The name of the repository after characters not allowed in a git repository name are replaced with
        /// dashes.
        /// </summary>
        string SafeRepositoryName { get; }
        ReactivePropertyValidator<string> RepositoryNameValidator { get; }
        ReactivePropertyValidator<string> SafeRepositoryNameWarningValidator { get; }

        string Description { get; set; }

        IReadOnlyList<IAccount> Accounts { get; }

        /// <summary>
        /// The account or organization that will be the owner of the created repository.
        /// </summary>
        IAccount SelectedAccount { get; set; }

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

        /// <summary>
        /// Command that opens a browser to a page for upgrading the user's plan.
        /// </summary>
        ICommand UpgradeAccountPlan { get; }
    }
}
