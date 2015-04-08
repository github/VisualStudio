using GitHub.Validation;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Represents a view model responsible for authenticating a user
    /// against a GitHub Enterprise instance.
    /// </summary>
    public interface ILoginToGitHubForEnterpriseViewModel : ILoginToGitHubViewModel
    {
        /// <summary>
        /// Gets or sets the URL to the GitHub Enterprise instance
        /// </summary>
        string EnterpriseUrl { get; set; }

        /// <summary>
        /// Gets the validator instance used for validating the 
        /// <see cref="EnterpriseUrl"/> property
        /// </summary>
        ReactivePropertyValidator EnterpriseUrlValidator { get; }
    }
}
