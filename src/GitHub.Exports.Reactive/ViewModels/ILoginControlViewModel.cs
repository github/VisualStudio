using System;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [Flags]
    public enum LoginMode
    {
        None = 0x00,
        DotComOnly = 0x01,
        EnterpriseOnly = 0x02,
        DotComOrEnterprise = 0x03,
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