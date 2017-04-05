using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.App;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using NLog;
using NullGuard;
using ReactiveUI;
using System.Diagnostics;
using System.Globalization;
using System.Reactive;
using GitHub.Extensions.Reactive;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.LogoutRequired)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LogoutRequiredViewModel : DialogViewModelBase, ILogoutRequiredViewModel
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly IRepositoryHosts repositoryHosts;
        readonly INotificationService notificationService;

        [ImportingConstructor]
        public LogoutRequiredViewModel(IRepositoryHosts repositoryHosts, INotificationService notificationService)
        {
            this.repositoryHosts = repositoryHosts;
            this.notificationService = notificationService;

            Title = Resources.LogoutRequiredTitle;
            Logout = ReactiveCommand.CreateAsyncObservable(OnLogout);
            Icon = Octicon.mark_github;
        }

        public override void Initialize([AllowNull] ViewWithData data)
        {
            if (data.MainFlow == UIControllerFlow.Gist)
            {
                Icon = Octicon.logo_gist;
                LogoutRequiredMessage = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.LogoutRequiredMessage,
                    Resources.LogoutRequiredFeatureGist);
            }
            else
                Debug.Assert(false, "Add a resource string for feature " + data.MainFlow + "!");
            base.Initialize(data);
        }

        public IReactiveCommand<ProgressState> Logout { get; }

        public override IObservable<Unit> Done
        {
            get { return Logout.Where(x => x == ProgressState.Success).SelectUnit(); }
        }

        IObservable<ProgressState> OnLogout(object unused)
        {
            return DoLogout().ToObservable()
                .Select(_ => ProgressState.Success)
                .Catch<ProgressState, Exception>(ex =>
                {
                    if (!ex.IsCriticalException())
                    {
                        log.Error(ex);
                        var error = StandardUserErrors.GetUserFriendlyErrorMessage(ex, ErrorType.LogoutFailed);
                        notificationService.ShowError(error);
                    }
                    return Observable.Return(ProgressState.Fail);
                });
        }

        async Task DoLogout()
        {
            if (repositoryHosts.GitHubHost?.SupportsGist == false)
            {
                await repositoryHosts.GitHubHost.LogOut();
            }

            if (repositoryHosts.EnterpriseHost?.SupportsGist == false)
            {
                await repositoryHosts.EnterpriseHost.LogOut();
            }
        }

        string logoutRequiredMessage;
        public string LogoutRequiredMessage
        {
            [return: AllowNull]
            get { return logoutRequiredMessage; }
            set { this.RaiseAndSetIfChanged(ref logoutRequiredMessage, value); }
        }

        Octicon icon;
        public Octicon Icon
        {
            get { return icon; }
            set { this.RaiseAndSetIfChanged(ref icon, value); }
        }
    }
}
