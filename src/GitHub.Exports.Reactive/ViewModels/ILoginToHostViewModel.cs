using System.Reactive;
using GitHub.Models;
using GitHub.Validation;
using ReactiveUI;
using ReactiveUI.Legacy;

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
        ReactiveCommand<Unit, IConnection> Login { get; }

        /// <summary>
        /// Gets a command which, when invoked, performs an OAuth login.
        /// </summary>
        ReactiveCommand<Unit, IConnection> LoginViaOAuth { get; }

        /// <summary>
        /// Gets a command which, when invoked, direct the user to a
        /// GitHub.com sign up flow
        /// </summary>
        ReactiveCommand<Unit, Unit> SignUp { get; }

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
        ReactiveCommand<Unit, Unit> Reset { get; }

#pragma warning disable CS0618 // Type or member is obsolete
        /// <summary>
        /// Gets a command which, when invoked, directs the user to
        /// a GitHub.com lost password flow.
        /// </summary>
        IRecoveryCommand NavigateForgotPassword { get; }
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
        /// <summary>
        /// Gets an error to display to the user.
        /// </summary>
        UserError Error { get; }
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Called when the login UI is hidden or dismissed.
        /// </summary>
        void Deactivated();
    }
}
