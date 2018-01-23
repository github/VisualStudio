using System;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Concurrency;
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
        readonly IEnterpriseCapabilitiesService enterpriseCapabilities;

        [ImportingConstructor]
        public LoginToGitHubForEnterpriseViewModel(
            IConnectionManager connectionManager,
            IEnterpriseCapabilitiesService enterpriseCapabilities,
            IVisualStudioBrowser browser)
            : this(connectionManager, enterpriseCapabilities, browser, Scheduler.Default)
        {
        }

        public LoginToGitHubForEnterpriseViewModel(
            IConnectionManager connectionManager,
            IEnterpriseCapabilitiesService enterpriseCapabilities,
            IVisualStudioBrowser browser,
            IScheduler scheduler)
            : base(connectionManager, browser)
        {
            Guard.ArgumentNotNull(connectionManager, nameof(connectionManager));
            Guard.ArgumentNotNull(enterpriseCapabilities, nameof(enterpriseCapabilities));
            Guard.ArgumentNotNull(browser, nameof(browser));

            this.enterpriseCapabilities = enterpriseCapabilities;

            EnterpriseUrlValidator = ReactivePropertyValidator.For(this, x => x.EnterpriseUrl)
                .IfNullOrEmpty(Resources.EnterpriseUrlValidatorEmpty)
                .IfNotUri(Resources.EnterpriseUrlValidatorInvalid)
                .IfGitHubDotComHost(Resources.EnterpriseUrlValidatorNotAGitHubHost);

            canLogin = this.WhenAnyValue(
                x => x.UsernameOrEmailValidator.ValidationResult.IsValid,
                x => x.PasswordValidator.ValidationResult.IsValid,
                x => x.SupportedLoginMethods,
                (x, y, z) => (x || (z & EnterpriseLoginMethods.Token) != 0) && y)
                .ToProperty(this, x => x.CanLogin);

            canSsoLogin = this.WhenAnyValue(
                x => x.EnterpriseUrlValidator.ValidationResult.IsValid)
                .ToProperty(this, x => x.CanLogin);

            this.WhenAnyValue(x => x.EnterpriseUrl, x => x.EnterpriseUrlValidator.ValidationResult)
                .Throttle(TimeSpan.FromMilliseconds(500), scheduler)
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
            if (string.IsNullOrWhiteSpace(UsernameOrEmail))
            {
                return LogInWithToken(HostAddress.Create(EnterpriseUrl), Password);
            }
            else
            {
                return LogInToHost(HostAddress.Create(EnterpriseUrl));
            }
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

        protected override Uri BaseUri => new UriBuilder(EnterpriseUrl).Uri;

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
                var uri = new UriBuilder(url).Uri;
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
                if ((loginMethods & EnterpriseLoginMethods.Token) != 0 &&
                    (loginMethods & EnterpriseLoginMethods.UsernameAndPassword) != 0)
                {
                    loginMethods &= ~EnterpriseLoginMethods.Token;
                }

                ProbeStatus = enterpriseInstance ? EnterpriseProbeStatus.Valid : EnterpriseProbeStatus.Invalid;
                SupportedLoginMethods = loginMethods;
            }
        }

        async Task<IConnection> LogInWithToken(HostAddress hostAddress, string token)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));

            if (await ConnectionManager.GetConnection(hostAddress) != null)
            {
                await ConnectionManager.LogOut(hostAddress);
            }

            return await ConnectionManager.LogInWithToken(hostAddress, token);
        }
    }
}
