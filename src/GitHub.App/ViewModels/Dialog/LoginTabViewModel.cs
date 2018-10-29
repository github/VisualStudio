using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHub.App;
using GitHub.Authentication;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Info;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.Validation;
using ReactiveUI;
using Serilog;
using IRecoveryCommand = ReactiveUI.Legacy.IRecoveryCommand;
using RecoveryCommand = ReactiveUI.Legacy.RecoveryCommand;
using RecoveryOptionResult = ReactiveUI.Legacy.RecoveryOptionResult;
using UserError = ReactiveUI.Legacy.UserError;

#pragma warning disable CS0618 // Type or member is obsolete

namespace GitHub.ViewModels.Dialog
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public abstract class LoginTabViewModel : ReactiveObject
    {
        static readonly ILogger log = LogManager.ForContext<LoginTabViewModel>();
        CancellationTokenSource oauthCancel;

        protected LoginTabViewModel(
            IConnectionManager connectionManager,
            IVisualStudioBrowser browser)
        {
            Guard.ArgumentNotNull(connectionManager, nameof(connectionManager));
            Guard.ArgumentNotNull(browser, nameof(browser));

            ConnectionManager = connectionManager;

            UsernameOrEmailValidator = ReactivePropertyValidator.For(this, x => x.UsernameOrEmail)
                .IfNullOrEmpty(Resources.UsernameOrEmailValidatorEmpty)
                .IfMatch(@"\s", Resources.UsernameOrEmailValidatorSpaces);

            PasswordValidator = ReactivePropertyValidator.For(this, x => x.Password)
                .IfNullOrEmpty(Resources.PasswordValidatorEmpty);

            canLogin = this.WhenAny(
                x => x.UsernameOrEmailValidator.ValidationResult.IsValid,
                x => x.PasswordValidator.ValidationResult.IsValid,
                (x, y) => x.Value && y.Value).ToProperty(this, x => x.CanLogin);

            Login = ReactiveCommand.CreateFromTask(LogIn, this.WhenAny(x => x.CanLogin, x => x.Value));
            Login.ThrownExceptions.Subscribe(HandleError);
            isLoggingIn = Login.IsExecuting.ToProperty(this, x => x.IsLoggingIn);

            LoginViaOAuth = ReactiveCommand.CreateFromTask(
                LogInViaOAuth,
                this.WhenAnyValue(x => x.IsLoggingIn, x => !x));
            LoginViaOAuth.ThrownExceptions.Subscribe(HandleError);

            Reset = ReactiveCommand.CreateFromTask(Clear);

            NavigateForgotPassword = new RecoveryCommand(Resources.ForgotPasswordLink, _ =>
            {
                browser.OpenUrl(new Uri(BaseUri, GitHubUrls.ForgotPasswordPath));
                return RecoveryOptionResult.RetryOperation;
            });

            SignUp = ReactiveCommand.CreateFromObservable(() =>
            {
                browser.OpenUrl(GitHubUrls.Plans);
                return Observable.Return(Unit.Default);
            });
        }
        protected IConnectionManager ConnectionManager { get; }
        protected abstract Uri BaseUri { get; }
        public ReactiveCommand<Unit, Unit> SignUp { get; }

        public ReactiveCommand<Unit, IConnection> Login { get; }
        public ReactiveCommand<Unit, IConnection> LoginViaOAuth { get; }
        public ReactiveCommand<Unit, Unit> Reset { get; }
#pragma warning disable CS0618 // Type or member is obsolete
        public IRecoveryCommand NavigateForgotPassword { get; }
#pragma warning restore CS0618 // Type or member is obsolete

        string usernameOrEmail;
        public string UsernameOrEmail
        {
            get { return usernameOrEmail; }
            set { this.RaiseAndSetIfChanged(ref usernameOrEmail, value); }
        }

        ReactivePropertyValidator usernameOrEmailValidator;
        public ReactivePropertyValidator UsernameOrEmailValidator
        {
            get { return usernameOrEmailValidator; }
            private set { this.RaiseAndSetIfChanged(ref usernameOrEmailValidator, value); }
        }

        string password;
        public string Password
        {
            get { return password; }
            set { this.RaiseAndSetIfChanged(ref password, value); }
        }

        ReactivePropertyValidator passwordValidator;
        public ReactivePropertyValidator PasswordValidator
        {
            get { return passwordValidator; }
            private set { this.RaiseAndSetIfChanged(ref passwordValidator, value); }
        }

        readonly ObservableAsPropertyHelper<bool> isLoggingIn;
        public bool IsLoggingIn
        {
            get { return isLoggingIn.Value; }
        }

        protected ObservableAsPropertyHelper<bool> canLogin;
        public bool CanLogin
        {
            get { return canLogin.Value; }
        }

        protected ObservableAsPropertyHelper<bool> canSsoLogin;
        public bool CanSsoLogin
        {
            get { return canSsoLogin.Value; }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        UserError error;
        public UserError Error
        {
            get { return error; }
            set { this.RaiseAndSetIfChanged(ref error, value); }
        }
#pragma warning restore CS0618 // Type or member is obsolete

        public void Deactivated() => oauthCancel?.Cancel();

        protected abstract Task<IConnection> LogIn();
        protected abstract Task<IConnection> LogInViaOAuth();

        protected async Task<IConnection> LogInToHost(HostAddress hostAddress)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            if (await ConnectionManager.GetConnection(hostAddress) != null)
            {
                await ConnectionManager.LogOut(hostAddress);
            }

            return await ConnectionManager.LogIn(hostAddress, UsernameOrEmail, Password);
        }

        protected async Task<IConnection> LoginToHostViaOAuth(HostAddress address)
        {
            oauthCancel = new CancellationTokenSource();

            if (await ConnectionManager.GetConnection(address) != null)
            {
                await ConnectionManager.LogOut(address);
            }

            try
            {
                return await ConnectionManager.LogInViaOAuth(address, oauthCancel.Token);
            }
            finally
            {
                oauthCancel.Dispose();
                oauthCancel = null;
            }
        }

        async Task Clear()
        {
            UsernameOrEmail = null;
            Password = null;
            await UsernameOrEmailValidator.ResetAsync();
            await PasswordValidator.ResetAsync();
            await ResetValidation();
        }

        protected virtual Task ResetValidation()
        {
            // noop
            return Task.FromResult(0);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        void HandleError(Exception ex)
        {
            // The Windows ERROR_OPERATION_ABORTED error code.
            const int operationAborted = 995;

            if (ex is HttpListenerException &&
                ((HttpListenerException)ex).ErrorCode == operationAborted)
            {
                // An Oauth listener was aborted, probably because the user closed the login
                // dialog or switched between the GitHub and Enterprise tabs while listening
                // for an Oauth callbacl.
                return;
            }

            if (ex.IsCriticalException()) return;

            log.Error(ex, "Error logging into '{BaseUri}' as '{UsernameOrEmail}'", BaseUri, UsernameOrEmail);
            if (ex is Octokit.ForbiddenException)
            {
                Error = new UserError(Resources.LoginFailedForbiddenMessage, ex.Message);
            }
            else
            {
                Error = new UserError(ex.Message);
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
