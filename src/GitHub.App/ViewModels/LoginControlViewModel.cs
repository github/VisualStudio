using System;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using GitHub.Authentication;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Info;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.Validation;
using NullGuard;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    [ExportViewModel(ViewType=UIViewType.Login)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoginControlViewModel : ReactiveValidatableObject, ILoginViewModel, IDisposable
    {
        readonly Lazy<IEnterpriseProbe> lazyEnterpriseProbe;
        const string notEnterpriseServerError = "Not an Enterprise server. Please enter an Enterprise URL";

        readonly ReactiveCommand<object> signUpCommand;
        readonly ReactiveCommand<object> forgotPasswordCommand;

        public ReactiveCommand<AuthenticationResult> LoginCommand { get; private set; }
        public ICommand LoginCmd { get { return LoginCommand; } }
        
        public ICommand ForgotPasswordCommand { get { return forgotPasswordCommand; } }
        public ReactiveCommand<object> ShowDotComLoginCommand { get; set; }
        public ReactiveCommand<object> ShowEnterpriseLoginCommand { get; set; }
        public ICommand SignUpCommand { get { return signUpCommand; } }

        string enterpriseUrl;
        [ValidateIf("IsLoggingInToEnterprise")]
        [Required(ErrorMessage = "Please enter an Enterprise URL")]
        [AllowNull]
        public string EnterpriseUrl
        {
            get { return enterpriseUrl; }
            set { this.RaiseAndSetIfChanged(ref enterpriseUrl, value); }
        }

        readonly ObservableAsPropertyHelper<bool> isLoggingInToEnterprise;
        public bool IsLoggingInToEnterprise
        {
            get { return isLoggingInToEnterprise.Value; }
        }

        readonly ObservableAsPropertyHelper<bool> isLoginInProgress;
        public bool IsLoginInProgress
        {
            get { return isLoginInProgress.Value; }
        }

        readonly ObservableAsPropertyHelper<string> loginButtonText;
        public string LoginButtonText
        {
            get { return loginButtonText.Value; }
        }

        bool loginFailed;
        public bool LoginFailed
        {
            get { return loginFailed; }
            set { this.RaiseAndSetIfChanged(ref loginFailed, value); }
        }

        string loginFailedText;
        public string LoginFailedText
        {
            get { return loginFailedText; }
            private set { this.RaiseAndSetIfChanged(ref loginFailedText, value); }
        }

        readonly ObservableAsPropertyHelper<LoginMode> loginMode;
        public LoginMode LoginMode
        {
            get { return loginMode.Value; }
        }

        public string LoginPrefix { get; set; }

        readonly ObservableAsPropertyHelper<LoginTarget> loginTarget;
        public LoginTarget LoginTarget
        {
            get { return loginTarget.Value; }
        }

        readonly ObservableAsPropertyHelper<VisualState> visualState;
        public VisualState VisualState
        {
            get { return visualState.Value; }
        }

        string password;
        [AllowNull]
        public string Password
        {
            [return: AllowNull]
            get { return password; }
            set { this.RaiseAndSetIfChanged(ref password, value); }
        }

        readonly ObservableAsPropertyHelper<Uri> forgotPasswordUrl;
        public Uri ForgotPasswordUrl
        {
            get { return forgotPasswordUrl.Value; }
        }

        Uri enterpriseHostBaseUrl;
        Uri EnterpriseHostBaseUrl
        {
            get { return enterpriseHostBaseUrl; }
            set { this.RaiseAndSetIfChanged(ref enterpriseHostBaseUrl, value); }
        }

        // HACKETY HACK!
        // Because #Bind() doesn't yet set up validation, we must use XAML bindings for username and password.
        // But, because our SecurePasswordBox manipulates base.Text, it doesn't work with XAML binding.
        // (It binds the password mask, not the password.)
        // So, this property is a "black hole" to point the XAML binding to so validation works.
        // And the actual password is bound to #Password via #Bind(). Ugly? Yep.
        [Required(ErrorMessage = "Please enter your password")]
        public string PasswordNoOp { get; set; }

        protected IRepositoryHosts RepositoryHosts { get; private set; }

        string usernameOrEmail;
        [Required(ErrorMessage = "Please enter your username or email address")]
        [AllowNull]
        public string UsernameOrEmail
        {
            [return: AllowNull]
            get { return usernameOrEmail; }
            set { this.RaiseAndSetIfChanged(ref usernameOrEmail, value); }
        }

        readonly Subject<AuthenticationResult> authenticationResults;
        public IObservable<AuthenticationResult> AuthenticationResults { get { return authenticationResults; } }

        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "It's Rx baby")]
        [ImportingConstructor]
        public LoginControlViewModel(
            IServiceProvider serviceProvider,
            IRepositoryHosts repositoryHosts,
            IBrowser browser,
            Lazy<IEnterpriseProbe> enterpriseProbe) : base(serviceProvider) 
        {
            RepositoryHosts = repositoryHosts;
            lazyEnterpriseProbe = enterpriseProbe;

            var canLogin = this.WhenAny(x => x.IsValid, x => x.Value);

            loginButtonText = this.WhenAny(x => x.LoginFailed, x => x.Value ? "Try again" : "Log in")
                .ToProperty(this, x => x.LoginButtonText, initialValue: "Log In");

            LoginPrefix = "Log in";
            LoginFailedText = "Log in failed";

            LoginCommand = ReactiveCommand.CreateAsyncObservable(canLogin,
                    x => Validate()
                        ? LogIn()
                        : Observable.Return(AuthenticationResult.CredentialFailure));

            LoginCommand.IsExecuting
                .ToProperty(this, vm => vm.IsLoginInProgress, out isLoginInProgress);

            signUpCommand = ReactiveCommand.Create(Observable.Return(true));
            signUpCommand.Subscribe(_ => browser.OpenUrl(GitHubUrls.Plans));

            // Whenever a host logs on or off we re-evaluate this. If there are no logged on hosts (local excluded)
            // then the user may log on to either .com or an enterprise instance. If there's already a logged on host
            // for .com then the user may only log on to an enterprise instance and vice versa.
            loginMode = Observable.CombineLatest(
                    this.WhenAny(x => x.RepositoryHosts.GitHubHost.IsLoggedIn, x => x.Value),
                    this.WhenAny(x => x.RepositoryHosts.EnterpriseHost.IsLoggedIn, x => x.Value),
                    GetLoginModeFromLoggedInRemoteHosts)
                .ToProperty(this, x => x.LoginMode);

            var canSwitchTargets = this.WhenAny(
                    x => x.IsLoginInProgress,
                    y => y.LoginMode,
                    (x, y) => new { IsLoginInProgress = x.Value, LoginMode = y.Value }
                )
                .Select(x => !x.IsLoginInProgress && x.LoginMode == LoginMode.DotComOrEnterprise);

            ShowDotComLoginCommand = ReactiveCommand.Create(canSwitchTargets);

            ShowEnterpriseLoginCommand = ReactiveCommand.Create(canSwitchTargets);

            loginTarget = Observable.Merge(
                    this.WhenAny(x => x.LoginMode, x => x.Value),
                    ShowEnterpriseLoginCommand.Select(_ => LoginMode.EnterpriseOnly),
                    ShowDotComLoginCommand.Select(_ => LoginMode.DotComOnly))
                .Where(x =>
                    x == LoginMode.DotComOnly 
                    || x == LoginMode.EnterpriseOnly 
                    || x == LoginMode.DotComOrEnterprise)
                .Select(x => x == LoginMode.EnterpriseOnly ? LoginTarget.Enterprise : LoginTarget.DotCom)
                .ToProperty(this, x => x.LoginTarget);

            Observable.Merge(
                ShowDotComLoginCommand,
                ShowEnterpriseLoginCommand)
                .Subscribe(_ =>
                {
                    UsernameOrEmail = "";
                    Password = "";
                    ResetValidation();
                });

            this.WhenAny(
                    x => x.LoginMode,
                    x => x.LoginTarget,
                    (x, y) => new {Mode = x.Value, Target = y.Value})
                .Select(x => GetStateNameFromModeAndTarget(x.Mode, x.Target))
                .ToProperty(this, x => x.VisualState, out visualState);

            this.WhenAny(
                x => x.LoginTarget, 
                x => x.Value == LoginTarget.Enterprise)
                .ToProperty(this, x => x.IsLoggingInToEnterprise, out isLoggingInToEnterprise);

            authenticationResults = new Subject<AuthenticationResult>();
            
            forgotPasswordUrl = this.WhenAny(
                    x => x.EnterpriseHostBaseUrl,
                    x => x.LoginTarget,
                    (x, y) => new { enterpriseBaseUrl = x.Value, loginTarget = y.Value })
                .Select(x => GetForgotPasswordUrl(x.enterpriseBaseUrl, x.loginTarget))
                .ToProperty(this, x => x.ForgotPasswordUrl, initialValue: GetForgotPasswordUrl(HostAddress.GitHubDotComHostAddress.WebUri, LoginTarget.DotCom));

            forgotPasswordCommand = ReactiveCommand.Create();
            forgotPasswordCommand.Subscribe(_ => browser.OpenUrl(ForgotPasswordUrl));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            authenticationResults.OnCompleted();
            authenticationResults.Dispose();
        }

        protected IObservable<AuthenticationResult> DoEnterpriseLogIn()
        {
            Uri uri;
            if (EnterpriseUrl.IsNullOrEmpty() || !Uri.TryCreate(EnterpriseUrl, UriKind.Absolute, out uri))
            {
                LoginFailed = true;
                SetErrorMessage("EnterpriseUrl", "Please enter a valid Enterprise URL");
                return Observable.Return(AuthenticationResult.CredentialFailure);
            }

            var enterpriseHostAddress = HostAddress.Create(uri);

            if (enterpriseHostAddress == HostAddress.GitHubDotComHostAddress)
            {
                LoginFailed = true;
                SetErrorMessage("EnterpriseUrl", notEnterpriseServerError);
                return Observable.Return(AuthenticationResult.CredentialFailure);
            }

            var enterpriseProbe = lazyEnterpriseProbe.Value;
            // Make a test request to /site/sha to make sure the provided URL points to a GitHub site:
            return enterpriseProbe.Probe(enterpriseHostAddress.WebUri)
                .SelectMany(result => HandleEnterpriseProbeResult(result, enterpriseHostAddress));
        }

        IObservable<AuthenticationResult> HandleEnterpriseProbeResult(
            EnterpriseProbeResult result,
            HostAddress enterpriseHostAddress)
        {
            LoginFailed = result != EnterpriseProbeResult.Ok;

            // Something went wrong with the test request, like it doesn't exist or timed out:
            if (result == EnterpriseProbeResult.Failed)
            {
                SetErrorMessage("EnterpriseUrl", "Failed to connect to the URL.");
                return Observable.Return(AuthenticationResult.CredentialFailure);
            }

            // The test request didn't fail, but it didn't return 200, so it's not a GitHub site:
            if (result == EnterpriseProbeResult.NotFound)
            {
                SetErrorMessage("EnterpriseUrl", notEnterpriseServerError);
                return Observable.Return(AuthenticationResult.CredentialFailure);
            }

            EnterpriseHostBaseUrl = enterpriseHostAddress.WebUri;

            var host = RepositoryHosts.RepositoryHostFactory.Create(enterpriseHostAddress);

            return DoLogIn(host)
                .Do(authenticationResult =>
                {
                    if (authenticationResult.IsSuccess())
                        RepositoryHosts.EnterpriseHost = host;
                });
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected IObservable<AuthenticationResult> DoLogIn(IRepositoryHost host)
        {
            LoginFailed = false;
            if (usernameOrEmail.IsNullOrEmpty() || password.IsNullOrEmpty())
            {
                LoginFailed = true;
                Password = "";
                return Observable.Return(AuthenticationResult.CredentialFailure);
            }

            return host.LogIn(usernameOrEmail, password)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<AuthenticationResult, Exception>(ex =>
                {
                    LoginFailed = true;
                    return Observable.Throw<AuthenticationResult>(ex);
                })
                .Do(result =>
                {
                    authenticationResults.OnNext(result);

                    if (result.IsSuccess())
                        return;

                    LoginFailed = true;
                    LoginFailedText = String.Format(
                        CultureInfo.InvariantCulture,
                        "{0} failed",
                        (result == AuthenticationResult.CredentialFailure ?
                            LoginPrefix :
                            "Two-factor authentication"));

                })
                .Finally(() =>
                {
                    Password = "";
                    ResetValidation();
                })
                .Multicast(new AsyncSubject<AuthenticationResult>()).RefCount();
        }

        private static LoginMode GetLoginModeFromLoggedInRemoteHosts(
            bool gitHubHostLoggedIn,
            bool enterpriseHostLoggedIn)
        {
            return gitHubHostLoggedIn
                ? enterpriseHostLoggedIn
                    ? LoginMode.None
                    : LoginMode.EnterpriseOnly
                : enterpriseHostLoggedIn // And Not logged into GitHub
                    ? LoginMode.DotComOnly
                    : LoginMode.DotComOrEnterprise;
        }

        public virtual IObservable<AuthenticationResult> LogIn()
        {
            return LoginTarget == LoginTarget.Enterprise
                ? DoEnterpriseLogIn()
                : DoLogIn(RepositoryHosts.GitHubHost);
        }

        public void Reset()
        {
            LoginPrefix = "Log in";
            LoginFailed = false;
            EnterpriseUrl = "";
            UsernameOrEmail = "";
            Password = "";
            ResetValidation();
        }

        static Uri GetForgotPasswordUrl(Uri validEnterpriseHostBaseUrl, LoginTarget currentLoginTarget)
        {
            var baseUrl = currentLoginTarget == LoginTarget.DotCom || validEnterpriseHostBaseUrl == null
                ? HostAddress.GitHubDotComHostAddress.WebUri
                : validEnterpriseHostBaseUrl;
            return new Uri(baseUrl, GitHubUrls.ForgotPasswordPath);
        }

        private static VisualState GetStateNameFromModeAndTarget(LoginMode loginMode, LoginTarget loginTarget)
        {
            if (loginMode == LoginMode.DotComOnly || loginMode == LoginMode.EnterpriseOnly)
                return (VisualState)loginMode;

            switch (loginTarget)
            {
                case LoginTarget.DotCom:
                case LoginTarget.Enterprise:
                    return (VisualState)loginTarget;
            }

            return VisualState.DotCom;
        }
    }

    public enum LoginMode
    {
        None = 0,
        DotComOrEnterprise,
        DotComOnly = 3,
        EnterpriseOnly = 4,
    }

    public enum LoginTarget
    {
        None = 0,
        DotCom = 1,
        Enterprise = 2,
    }

    public enum VisualState
    {
        None = 0,
        DotCom = 1,
        Enterprise = 2,
        DotComOnly = 3,
        EnterpriseOnly = 4
    }
}
