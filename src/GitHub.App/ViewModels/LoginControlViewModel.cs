using System;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.App;
using GitHub.Authentication;
using GitHub.Exports;
using GitHub.Extensions.Reactive;
using GitHub.Primitives;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.Login)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoginControlViewModel : DialogViewModelBase, ILoginControlViewModel
    {
        [ImportingConstructor]
        public LoginControlViewModel(
            IConnectionManager connectionManager,
            ILoginToGitHubViewModel loginToGitHubViewModel,
            ILoginToGitHubForEnterpriseViewModel loginToGitHubEnterpriseViewModel)
        {
            Title = Resources.LoginTitle;
            ConnectionManager = connectionManager;
            GitHubLogin = loginToGitHubViewModel;
            EnterpriseLogin = loginToGitHubEnterpriseViewModel;

            isLoginInProgress = this.WhenAny(
                x => x.GitHubLogin.IsLoggingIn,
                x => x.EnterpriseLogin.IsLoggingIn,
                (x, y) => x.Value || y.Value
            ).ToProperty(this, vm => vm.IsLoginInProgress);

            UpdateLoginMode();
            connectionManager.Connections.CollectionChanged += (_, __) => UpdateLoginMode();

            AuthenticationResults = Observable.Merge(
                loginToGitHubViewModel.Login,
                loginToGitHubViewModel.LoginViaOAuth,
                EnterpriseLogin.Login);
        }

        ILoginToGitHubViewModel github;
        public ILoginToGitHubViewModel GitHubLogin
        {
            get { return github; }
            set { this.RaiseAndSetIfChanged(ref github, value); }
        }

        ILoginToGitHubForEnterpriseViewModel githubEnterprise;
        public ILoginToGitHubForEnterpriseViewModel EnterpriseLogin
        {
            get { return githubEnterprise; }
            set { this.RaiseAndSetIfChanged(ref githubEnterprise, value); }
        }

        IConnectionManager connectionManager;
        public IConnectionManager ConnectionManager
        {
            get { return connectionManager; }
            set { this.RaiseAndSetIfChanged(ref connectionManager, value); }
        }

        LoginMode loginMode;
        public LoginMode LoginMode
        {
            get { return loginMode; }
            private set { this.RaiseAndSetIfChanged(ref loginMode, value); }
        }

        readonly ObservableAsPropertyHelper<bool> isLoginInProgress;
        public bool IsLoginInProgress { get { return isLoginInProgress.Value; } }

        public IObservable<AuthenticationResult> AuthenticationResults { get; private set; }

        public override IObservable<Unit> Done
        {
            get { return AuthenticationResults.Where(x => x == AuthenticationResult.Success).SelectUnit(); }
        }

        void UpdateLoginMode()
        {
            var result = LoginMode.DotComOrEnterprise;

            foreach (var connection in connectionManager.Connections)
            {
                if (connection.IsLoggedIn)
                {
                    result &= ~((connection.HostAddress == HostAddress.GitHubDotComHostAddress) ?
                        LoginMode.DotComOnly : LoginMode.EnterpriseOnly);
                }
            }

            LoginMode = result;
        }
    }
}
