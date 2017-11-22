using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using GitHub.App;
using GitHub.Authentication;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Info;
using GitHub.Logging;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.Validation;
using ReactiveUI;
using Serilog;

namespace GitHub.ViewModels
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

            Login = ReactiveCommand.CreateAsyncObservable(this.WhenAny(x => x.CanLogin, x => x.Value), LogIn);
            Login.ThrownExceptions.Subscribe(HandleError);

            LoginViaOAuth = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.IsLoggingIn, x => !x),
                LogInViaOAuth);
            LoginViaOAuth.ThrownExceptions.Subscribe(HandleError);

            isLoggingIn = Login.IsExecuting.ToProperty(this, x => x.IsLoggingIn);

            Reset = ReactiveCommand.CreateAsyncTask(_ => Clear());

            NavigateForgotPassword = new RecoveryCommand(Resources.ForgotPasswordLink, _ =>
            {
                browser.OpenUrl(new Uri(BaseUri, GitHubUrls.ForgotPasswordPath));
                return RecoveryOptionResult.RetryOperation;
            });

            SignUp = ReactiveCommand.CreateAsyncObservable(_ =>
            {
                browser.OpenUrl(GitHubUrls.Plans);
                return Observable.Return(Unit.Default);
            });
        }
        protected IConnectionManager ConnectionManager { get; }
        protected abstract Uri BaseUri { get; }
        public IReactiveCommand<Unit> SignUp { get; }

        public IReactiveCommand<AuthenticationResult> Login { get; }
        public IReactiveCommand<AuthenticationResult> LoginViaOAuth { get; }
        public IReactiveCommand<Unit> Reset { get; }
        public IRecoveryCommand NavigateForgotPassword { get; }

        string usernameOrEmail;
        public string UsernameOrEmail
        {
            get
            { return usernameOrEmail; }
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
            get
            { return password; }
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

        UserError error;
        public UserError Error
        {
            get { return error; }
            set { this.RaiseAndSetIfChanged(ref error, value); }
        }

        public void Deactivated() => oauthCancel?.Cancel();

        protected abstract IObservable<AuthenticationResult> LogIn(object args);
        protected abstract Task<AuthenticationResult> LogInViaOAuth(object args);

        protected IObservable<AuthenticationResult> LogInToHost(HostAddress hostAddress)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            return Observable.Defer(async () =>
            {
                if (hostAddress != null)
                {
                    if (await ConnectionManager.GetConnection(hostAddress) != null)
                    {
                        await ConnectionManager.LogOut(hostAddress);
                    }

                    await ConnectionManager.LogIn(hostAddress, UsernameOrEmail, Password);
                    return Observable.Return(AuthenticationResult.Success);
                }

                return Observable.Return(AuthenticationResult.CredentialFailure);
            })
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(authResult => {
                switch (authResult)
                {
                    case AuthenticationResult.CredentialFailure:
                        Error = new UserError(
                            Resources.LoginFailedText,
                            Resources.LoginFailedMessage,
                            new[] { NavigateForgotPassword });
                        break;
                    case AuthenticationResult.VerificationFailure:
                        break;
                    case AuthenticationResult.EnterpriseServerNotFound:
                        Error = new UserError(Resources.CouldNotConnectToGitHub);
                        break;
                }
            })
            .SelectMany(authResult =>
            {
                switch (authResult)
                {
                    case AuthenticationResult.CredentialFailure:
                    case AuthenticationResult.EnterpriseServerNotFound:
                    case AuthenticationResult.VerificationFailure:
                        Password = "";
                        return Observable.FromAsync(PasswordValidator.ResetAsync)
                            .Select(_ => AuthenticationResult.CredentialFailure);
                    case AuthenticationResult.Success:
                        return Reset.ExecuteAsync()
                            .ContinueAfter(() => Observable.Return(AuthenticationResult.Success));
                    default:
                        return Observable.Throw<AuthenticationResult>(
                            new InvalidOperationException("Unknown EnterpriseLoginResult: " + authResult));
                }
            });
        }

        protected async Task LoginToHostViaOAuth(HostAddress address)
        {
            oauthCancel = new CancellationTokenSource();

            try
            {
                await ConnectionManager.LogInViaOAuth(address, oauthCancel.Token);
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

            log.Information(ex, "Error logging into '{BaseUri}' as '{UsernameOrEmail}'", BaseUri, UsernameOrEmail);
            if (ex is Octokit.ForbiddenException)
            {
                Error = new UserError(Resources.LoginFailedForbiddenMessage, ex.Message);
            }
            else
            {
                Error = new UserError(ex.Message);
            }
        }
    }
}
