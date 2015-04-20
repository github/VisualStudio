using System;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Authentication;
using GitHub.Info;
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

            NavigateLearnMore = ReactiveCommand.CreateAsyncObservable(_ =>
            {
                browser.OpenUrl(GitHubUrls.LearnMore);
                return Observable.Return(Unit.Default);

            });
        }

        readonly ObservableAsPropertyHelper<bool> canLogin;
        public override bool CanLogin
        {
            get { return canLogin.Value; }
        }

        protected override IObservable<AuthenticationResult> LogIn(object args)
        {
            return LogInToHost(HostAddress.Create(EnterpriseUrl));
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

        protected override Uri BaseUri
        {
            get
            {
                return new Uri(EnterpriseUrl);
            }
        }

        public IReactiveCommand<Unit> NavigateLearnMore
        {
            get;
            private set;
        }

        protected override async Task ResetValidation()
        {
            EnterpriseUrl = null;
            await EnterpriseUrlValidator.ResetAsync();
            ShowConnectingToHostFailed = false;
        }
    }
}
