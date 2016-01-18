using System.Reactive;
using GitHub.Authentication;
using GitHub.Validation;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface ILoginToHostViewModel
    {
        /// <summary>
        /// Gets or sets the sign in handle, can be either username or email.
        /// </summary>
        string UsernameOrEmail { get; set; }

        /// <summary>
        /// Gets the validator instance used for validating the 
        /// <see cref="UsernameOrEmail"/> property
        /// </summary>
        ReactivePropertyValidator UsernameOrEmailValidator { get; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// Gets the validator instance used for validating the 
        /// <see cref="Password"/> property
        /// </summary>
        ReactivePropertyValidator PasswordValidator { get; }

        /// <summary>
        /// Gets a command which, when invoked, performs the actual 
        /// login procedure.
        /// </summary>
        IReactiveCommand<AuthenticationResult> Login { get; }

        /// <summary>
        /// Gets a command which, when invoked, direct the user to a
        /// GitHub.com sign up flow
        /// </summary>
        IReactiveCommand<Unit> SignUp { get; }

        /// <summary>
        /// Gets a value indicating whether all validators pass and we
        /// can proceed with login.
        /// </summary>
        bool CanLogin { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is currently
        /// in the process of logging in.
        /// </summary>
        bool IsLoggingIn { get; }

        /// <summary>
        /// Gets a value indicating whether to show log an error
        /// message due to a failed log in.
        /// </summary>
        bool ShowLogInFailedError { get; }

        /// <summary>
        /// The message to show if login failed.
        /// </summary>
        string LoginFailedMessage { get; }

        /// <summary>
        /// Gets a command which, when invoked, resets all properties 
        /// and validators.
        /// </summary>
        IReactiveCommand<Unit> Reset { get; }

        /// <summary>
        /// Gets a command which, when invoked, directs the user to
        /// a GitHub.com lost password flow.
        /// </summary>
        IReactiveCommand<Unit> NavigateForgotPassword { get; }

        /// <summary>
        /// Gets a value indicating whether to show an error message
        /// due to being unable to connect to the host.
        /// </summary>
        bool ShowConnectingToHostFailed { get; }
    }
}
