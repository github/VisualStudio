using System;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.App;
using GitHub.Extensions;
using GitHub.Info;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.Validation;
using Octokit;
using ReactiveUI;
using IConnection = GitHub.Models.IConnection;

namespace GitHub.ViewModels.Dialog
{
    [Export(typeof(ILoginToGitHubForEnterpriseViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoginToGitHubForEnterpriseViewModel : LoginTabViewModel, ILoginToGitHubForEnterpriseViewModel
    {
        readonly ISimpleApiClientFactory apiClientFactory;
        readonly IEnterpriseCapabilitiesService enterpriseCapabilities;

        [ImportingConstructor]
        public LoginToGitHubForEnterpriseViewModel(
            IConnectionManager connectionManager,
            ISimpleApiClientFactory apiClientFactory,
            IEnterpriseCapabilitiesService enterpriseCapabilities,
            IVisualStudioBrowser browser)
            : base(connectionManager, browser)
        {
            Guard.ArgumentNotNull(connectionManager, nameof(connectionManager));
            Guard.ArgumentNotNull(apiClientFactory, nameof(apiClientFactory));
            Guard.ArgumentNotNull(enterpriseCapabilities, nameof(enterpriseCapabilities));
            Guard.ArgumentNotNull(browser, nameof(browser));

            this.apiClientFactory = apiClientFactory;
            this.enterpriseCapabilities = enterpriseCapabilities;

            EnterpriseUrlValidator = ReactivePropertyValidator.For(this, x => x.EnterpriseUrl)
                .IfNullOrEmpty(Resources.EnterpriseUrlValidatorEmpty)
                .IfNotUri(Resources.EnterpriseUrlValidatorInvalid)
                .IfGitHubDotComHost(Resources.EnterpriseUrlValidatorNotAGitHubHost);

            canLogin = this.WhenAny(
                x => x.UsernameOrEmailValidator.ValidationResult.IsValid,
                x => x.PasswordValidator.ValidationResult.IsValid,
                x => x.EnterpriseUrlValidator.ValidationResult.IsValid,
                (x, y, z) => x.Value && y.Value && z.Value)
                .ToProperty(this, x => x.CanLogin);

            canSsoLogin = this.WhenAnyValue(
                x => x.EnterpriseUrlValidator.ValidationResult.IsValid)
                .ToProperty(this, x => x.CanLogin);

            this.WhenAnyValue(x => x.EnterpriseUrl, x => x.EnterpriseUrlValidator.ValidationResult)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => EnterpriseUrlChanged(x.Item1, x.Item2?.IsValid ?? false));

            NavigateLearnMore = ReactiveCommand.CreateAsyncObservable(_ =>
            {
                browser.OpenUrl(GitHubUrls.LearnMore);
                return Observable.Return(Unit.Default);
            });
        }

        protected override Task<IConnection> LogIn(object args)
        {
            return LogInToHost(HostAddress.Create(EnterpriseUrl));
        }

        protected override Task<IConnection> LogInViaOAuth(object args)
        {
            return LoginToHostViaOAuth(HostAddress.Create(EnterpriseUrl));
        }

        string enterpriseUrl;
        public string EnterpriseUrl
        {
            get { return enterpriseUrl; }
            set { this.RaiseAndSetIfChanged(ref enterpriseUrl, value); }
        }

        EnterpriseProbeStatus probeStatus;
        public EnterpriseProbeStatus ProbeStatus
        {
            get { return probeStatus; }
            private set { this.RaiseAndSetIfChanged(ref probeStatus, value); }
        }

        EnterpriseLoginMethods? supportedLoginMethods;
        public EnterpriseLoginMethods? SupportedLoginMethods
        {
            get { return supportedLoginMethods; }
            private set { this.RaiseAndSetIfChanged(ref supportedLoginMethods, value); }
        }

        public ReactivePropertyValidator EnterpriseUrlValidator { get; }

        protected override Uri BaseUri => new Uri(EnterpriseUrl);

        public IReactiveCommand<Unit> NavigateLearnMore
        {
            get;
        }

        protected override async Task ResetValidation()
        {
            EnterpriseUrl = null;
            await EnterpriseUrlValidator.ResetAsync();
        }

        async void EnterpriseUrlChanged(string url, bool valid)
        {
            if (!valid)
            {
                // The EnterpriseUrlValidator will display an adorner in this case so don't show anything.
                ProbeStatus = EnterpriseProbeStatus.None;
                SupportedLoginMethods = null;
                return;
            }

            var enterpriseInstance = false;
            var loginMethods = (EnterpriseLoginMethods?)null;

            try
            {
                var uri = new Uri(url);
                ProbeStatus = EnterpriseProbeStatus.Checking;

                if (await enterpriseCapabilities.Probe(uri) == EnterpriseProbeResult.Ok)
                {
                    loginMethods = await enterpriseCapabilities.ProbeLoginMethods(uri);
                    enterpriseInstance = true;
                }
            }
            catch
            {
                ProbeStatus = EnterpriseProbeStatus.Invalid;
                loginMethods = null;
            }

            if (url == EnterpriseUrl)
            {
                ProbeStatus = enterpriseInstance ? EnterpriseProbeStatus.Valid : EnterpriseProbeStatus.Invalid;
                SupportedLoginMethods = loginMethods;
            }
        }
    }
}
