using System.ComponentModel;
using System.Reactive;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public enum LoginMode
    {
        None = 0,
        DotComOrEnterprise,
        DotComOnly = 3,
        EnterpriseOnly = 4,
    }

    /// <summary>
    /// Represents a view model responsible for providing log in to
    /// either GitHub.com or a GitHub Enterprise instance.
    /// </summary>
    public interface ILoginControlViewModel : IReactiveObject, ILoginViewModel
    {
        /// <summary>
        /// Gets a value indicating the currently available login modes 
        /// for the control.
        /// </summary>
        LoginMode LoginMode { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is currently
        /// in the process of logging in.
        /// </summary>
        bool IsLoginInProgress { get; }

        /// <summary>
        /// Gets a view model responsible for authenticating a user
        /// against GitHub.com.
        /// </summary>
        ILoginToGitHubViewModel GitHubLogin { get; }

        /// <summary>
        /// Gets a view model responsible for authenticating a user
        /// against a GitHub Enterprise instance.
        /// </summary>
        ILoginToGitHubForEnterpriseViewModel EnterpriseLogin { get; }
    }
}