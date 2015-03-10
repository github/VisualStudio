using System;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using GitHub.Authentication;
using GitHub.Extensions;
using GitHub.Info;
using GitHub.Models;
using GitHub.Services;
using GitHub.Validation;
using NLog;
using NullGuard;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public abstract class LoginTabViewModel : ReactiveObject
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected LoginTabViewModel(IRepositoryHosts repositoryHosts, IBrowser browser)
        {
            RepositoryHosts = repositoryHosts;

            UsernameOrEmailValidator = ReactivePropertyValidator.For(this, x => x.UsernameOrEmail)
                .IfNullOrEmpty("Please enter your username or email address")
                .IfMatch(@"\s", "Username or email address must not have spaces");

            PasswordValidator = ReactivePropertyValidator.For(this, x => x.Password)
                .IfNullOrEmpty("Please enter your password");

            canLogin = this.WhenAny(
                x => x.UsernameOrEmailValidator.ValidationResult.IsValid,
                x => x.PasswordValidator.ValidationResult.IsValid,
                (x, y) => x.Value && y.Value).ToProperty(this, x => x.CanLogin);

            Login = ReactiveCommand.CreateAsyncObservable(this.WhenAny(x => x.CanLogin, x => x.Value), LogIn);

            Login.ThrownExceptions.Subscribe(ex =>
            {
                if (!ex.IsCriticalException())
                {
                    log.Error(String.Format(CultureInfo.InvariantCulture, "Error logging into GitHub.com the website'{0}'", UsernameOrEmail), ex);
                    // TODO: Handle this error properly
                    MessageBox.Show(ex.Message);
                }
            });

            isLoggingIn = Login.IsExecuting.ToProperty(this, x => x.IsLoggingIn);

            Reset = ReactiveCommand.CreateAsyncTask(_ => Clear());

            ForgotPassword = ReactiveCommand.CreateAsyncObservable(_ =>
            {
                browser.OpenUrl(new Uri(BaseUri, GitHubUrls.ForgotPasswordPath));
                return Observable.Return(Unit.Default);
            });

            SignUp = ReactiveCommand.CreateAsyncObservable(_ =>
            {
                browser.OpenUrl(GitHubUrls.Plans);
                return Observable.Return(Unit.Default);
            });
        }
        protected IRepositoryHosts RepositoryHosts { get; private set; }
        protected abstract Uri BaseUri { get; }
        protected virtual IObservable<bool> CanLoginObservable { get; private set; }
        public IReactiveCommand<Unit> SignUp { get; private set; }

        public IReactiveCommand<AuthenticationResult> Login { get; private set; }
        public IReactiveCommand<Unit> Reset { get; private set; }
        public IReactiveCommand<Unit> ForgotPassword { get; private set; }

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

        readonly ObservableAsPropertyHelper<bool> canLogin;
        public virtual bool CanLogin
        {
            get { return canLogin.Value; }
        }

        bool showLogInFailedError;
        public bool ShowLogInFailedError
        {
            get { return showLogInFailedError; }
            protected set { this.RaiseAndSetIfChanged(ref showLogInFailedError, value); }
        }

        bool showTwoFactorAuthFailed;
        public bool ShowTwoFactorAuthFailedError
        {
            get { return showTwoFactorAuthFailed; }
            set { this.RaiseAndSetIfChanged(ref showTwoFactorAuthFailed, value); }
        }

        protected abstract IObservable<AuthenticationResult> LogIn(object args);

        async Task Clear()
        {
            UsernameOrEmail = null;
            Password = null;
            await UsernameOrEmailValidator.ResetAsync();
            await PasswordValidator.ResetAsync();
            await ResetValidation();

            ShowLogInFailedError = false;
            ShowTwoFactorAuthFailedError = false;
        }

        protected virtual Task ResetValidation()
        {
            // noop
            return Task.FromResult(0);
        }
    }
}
