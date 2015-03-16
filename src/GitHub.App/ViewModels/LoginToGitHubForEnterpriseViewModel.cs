using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Authentication;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.Validation;
using NullGuard;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [Export(typeof(ILoginToGitHubForEnterpriseViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoginToGitHubForEnterpriseViewModel : LoginTabViewModel, ILoginToGitHubForEnterpriseViewModel
    {
        [ImportingConstructor]
        public LoginToGitHubForEnterpriseViewModel(IRepositoryHosts hosts, IVisualStudioBrowser browser) : base(hosts, browser)
        {
            enterpriseUrlValidator = ReactivePropertyValidator.For(this, x => x.EnterpriseUrl)
                .IfNullOrEmpty("Please enter an Enterprise URL")
                .IfNotUri("Please enter a valid Enterprise URL")
                .IfGitHubDotComHost("Not an Enterprise server. Please enter an Enterprise URL");

            canLogin = this.WhenAny(
                x => x.UsernameOrEmailValidator.ValidationResult.IsValid,
                x => x.PasswordValidator.ValidationResult.IsValid,
                x => x.EnterpriseUrlValidator.ValidationResult.IsValid,
                (x, y, z) => x.Value && y.Value && z.Value)
                .ToProperty(this, x => x.CanLogin);
        }

        readonly ObservableAsPropertyHelper<bool> canLogin;
        public override bool CanLogin
        {
            get { return canLogin.Value; }
        }

        protected override IObservable<AuthenticationResult> LogIn(object args)
        {
            return Observable.Defer(() =>
            {
                ShowLogInFailedError = false;
                ShowTwoFactorAuthFailedError = false;
                ShowConnectingToHostFailed = false;

                var hostAddress = HostAddress.Create(EnterpriseUrl);
                return hostAddress != null ?
                    RepositoryHosts.LogInEnterpriseHost(hostAddress, UsernameOrEmail, Password)
                    : Observable.Return(AuthenticationResult.CredentialFailure);
            })
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(authResult => {
                switch (authResult)
                {
                    case AuthenticationResult.CredentialFailure:
                        ShowLogInFailedError = true;
                        break;
                    case AuthenticationResult.VerificationFailure:
                        ShowTwoFactorAuthFailedError = true;
                        break;
                    case AuthenticationResult.EnterpriseServerNotFound:
                        ShowConnectingToHostFailed = true;
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

        string enterpriseUrl;
        [AllowNull]
        public string EnterpriseUrl
        {
            [return: AllowNull]
            get { return enterpriseUrl; }
            set { this.RaiseAndSetIfChanged(ref enterpriseUrl, value); }
        }

        readonly ReactivePropertyValidator enterpriseUrlValidator;
        public ReactivePropertyValidator EnterpriseUrlValidator
        {
            get { return enterpriseUrlValidator; }
        }

        bool showConnectingToHostFailed;
        public bool ShowConnectingToHostFailed
        {
            get { return showConnectingToHostFailed; }
            set { this.RaiseAndSetIfChanged(ref showConnectingToHostFailed, value); }
        }

        protected override Uri BaseUri
        {
            get
            {
                return new Uri(EnterpriseUrl);
            }
        }

        protected override async Task ResetValidation()
        {
            EnterpriseUrl = null;
            await EnterpriseUrlValidator.ResetAsync();
            ShowConnectingToHostFailed = false;
        }
    }
}
