using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.App;
using GitHub.Authentication;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Info;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.Validation;
using NLog;
using NullGuard;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public abstract class LoginTabViewModel : ReactiveObject
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected LoginTabViewModel(IRepositoryHosts repositoryHosts, IVisualStudioBrowser browser)
        {
            RepositoryHosts = repositoryHosts;

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

            Login.ThrownExceptions.Subscribe(ex =>
            {
                if (ex.IsCriticalException()) return;

                log.Info(string.Format(CultureInfo.InvariantCulture, "Error logging into '{0}' as '{1}'", BaseUri, UsernameOrEmail), ex);

                if (ex is Octokit.ForbiddenException)
                {
                    Error = new UserError(Resources.LoginFailedForbiddenMessage, ex.Message);
                }
                else
                {
                    Error = new UserError(ex.Message);
                }
            });

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
        protected IRepositoryHosts RepositoryHosts { get; }
        protected abstract Uri BaseUri { get; }
        public IReactiveCommand<Unit> SignUp { get; }

        public IReactiveCommand<AuthenticationResult> Login { get; }
        public IReactiveCommand<Unit> Reset { get; }
        public IRecoveryCommand NavigateForgotPassword { get; }

        string usernameOrEmail;
        [AllowNull]
        public string UsernameOrEmail
        {
            [return: AllowNull]
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
        [AllowNull]
        public string Password
        {
            [return: AllowNull]
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

        UserError error;
        [AllowNull]
        public UserError Error
        {
            [return: AllowNull]
            get { return error; }
            set { this.RaiseAndSetIfChanged(ref error, value); }
        }

        protected abstract IObservable<AuthenticationResult> LogIn(object args);

        protected IObservable<AuthenticationResult> LogInToHost(HostAddress hostAddress)
        {
            return Observable.Defer(() =>
            {
                return hostAddress != null ?
                    RepositoryHosts.LogIn(hostAddress, UsernameOrEmail, Password)
                    : Observable.Return(AuthenticationResult.CredentialFailure);
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
    }
}
