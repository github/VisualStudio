using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.App;
using GitHub.Authentication;
using GitHub.Exports;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.Login)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoginControlViewModel : DialogViewModelBase, ILoginControlViewModel
    {
        [ImportingConstructor]
        public LoginControlViewModel(
            IRepositoryHosts hosts,
            ILoginToGitHubViewModel loginToGitHubViewModel,
            ILoginToGitHubForEnterpriseViewModel loginToGitHubEnterpriseViewModel)
        {
            Title = Resources.LoginTitle;
            RepositoryHosts = hosts;
            GitHubLogin = loginToGitHubViewModel;
            EnterpriseLogin = loginToGitHubEnterpriseViewModel;

            isLoginInProgress = this.WhenAny(
                x => x.GitHubLogin.IsLoggingIn,
                x => x.EnterpriseLogin.IsLoggingIn,
                (x, y) => x.Value || y.Value
            ).ToProperty(this, vm => vm.IsLoginInProgress);

            loginMode = this.WhenAny(
                x => x.RepositoryHosts.GitHubHost.IsLoggedIn,
                x => x.RepositoryHosts.EnterpriseHost.IsLoggedIn,
                (x, y) =>
                {
                    var canLogInToGitHub = x.Value == false;
                    var canLogInToEnterprise = y.Value == false;

                    return canLogInToGitHub && canLogInToEnterprise ? LoginMode.DotComOrEnterprise
                        : canLogInToGitHub ? LoginMode.DotComOnly
                        : canLogInToEnterprise ? LoginMode.EnterpriseOnly
                        : LoginMode.None;

                }).ToProperty(this, x => x.LoginMode);

            AuthenticationResults = Observable.Merge(
                loginToGitHubViewModel.Login,
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

        IRepositoryHosts repositoryHosts;
        public IRepositoryHosts RepositoryHosts
        {
            get { return repositoryHosts; }
            set { this.RaiseAndSetIfChanged(ref repositoryHosts, value); }
        }

        readonly ObservableAsPropertyHelper<LoginMode> loginMode;
        public LoginMode LoginMode { get { return loginMode.Value; } }

        readonly ObservableAsPropertyHelper<bool> isLoginInProgress;
        public bool IsLoginInProgress { get { return isLoginInProgress.Value; } }

        public IObservable<AuthenticationResult> AuthenticationResults { get; private set; }
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
