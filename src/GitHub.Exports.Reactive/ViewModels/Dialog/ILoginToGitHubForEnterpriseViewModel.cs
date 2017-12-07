using System.Reactive;
using GitHub.Validation;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    /// <summary>
    /// Represents a view model responsible for authenticating a user
    /// against a GitHub Enterprise instance.
    /// </summary>
    public interface ILoginToGitHubForEnterpriseViewModel : ILoginToHostViewModel
    {
        /// <summary>
        /// Gets or sets the URL to the GitHub Enterprise instance
        /// </summary>
        string EnterpriseUrl { get; set; }

        /// <summary>
        /// Gets a value indicating whether a GitHub Enterprise instance was found at
        /// <see cref="EnterpriseUrl"/>.
        /// </summary>
        bool? IsEnterpriseInstance { get; }

        /// <summary>
        /// Gets a value indcating whether the GitHub Enterprise instance at <see cref="EnterpriseUrl"/>
        /// supports logging in with a username and password.
        /// </summary>
        bool? SupportsUserNameAndPassword { get; }

        /// <summary>
        /// Gets the validator instance used for validating the 
        /// <see cref="EnterpriseUrl"/> property
        /// </summary>
        ReactivePropertyValidator EnterpriseUrlValidator { get; }

        /// <summary>
        /// Gets a command which, when invoked, directs the user to a learn more page
        /// </summary>
        IReactiveCommand<Unit> NavigateLearnMore { get; }
    }
}
