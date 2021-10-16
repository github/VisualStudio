using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.App;
using GitHub.Primitives;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    [Export(typeof(ILoginCredentialsViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoginCredentialsViewModel : ViewModelBase, ILoginCredentialsViewModel
    {
        [ImportingConstructor]
        public LoginCredentialsViewModel(
            IConnectionManager connectionManager,
            ILoginToGitHubViewModel loginToGitHubViewModel,
            ILoginToGitHubForEnterpriseViewModel loginToGitHubEnterpriseViewModel)
        {
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

            Done = Observable.Merge(
                loginToGitHubViewModel.Login,
                loginToGitHubViewModel.LoginViaOAuth,
                loginToGitHubEnterpriseViewModel.Login,
                loginToGitHubEnterpriseViewModel.LoginViaOAuth)
                .Where(x => x != null);
        }

        public string Title => Resources.LoginTitle;
        public ILoginToGitHubViewModel GitHubLogin { get; }
        public ILoginToGitHubForEnterpriseViewModel EnterpriseLogin { get; }
        public IConnectionManager ConnectionManager { get; }

        LoginMode loginMode;
        public LoginMode LoginMode
        {
            get { return loginMode; }
            private set { this.RaiseAndSetIfChanged(ref loginMode, value); }
        }

        readonly ObservableAsPropertyHelper<bool> isLoginInProgress;
        public bool IsLoginInProgress { get { return isLoginInProgress.Value; } }

        public IObservable<object> Done { get; }

        void UpdateLoginMode()
        {
            var result = LoginMode.DotComOrEnterprise;

            foreach (var connection in ConnectionManager.Connections)
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
