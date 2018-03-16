using System.Reactive;
using GitHub.Models;
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
        IReactiveCommand<IConnection> Login { get; }

        /// <summary>
        /// Gets a command which, when invoked, performs an OAuth login.
        /// </summary>
        IReactiveCommand<IConnection> LoginViaOAuth { get; }

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
        /// Gets a command which, when invoked, resets all properties 
        /// and validators.
        /// </summary>
        IReactiveCommand<Unit> Reset { get; }

        /// <summary>
        /// Gets a command which, when invoked, directs the user to
        /// a GitHub.com lost password flow.
        /// </summary>
        IRecoveryCommand NavigateForgotPassword { get; }

        /// <summary>
        /// Gets an error to display to the user.
        /// </summary>
        UserError Error { get; }

        /// <summary>
        /// Called when the login UI is hidden or dismissed.
        /// </summary>
        void Deactivated();
    }
}
