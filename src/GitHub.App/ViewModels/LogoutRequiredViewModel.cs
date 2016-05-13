using GitHub.Api;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using NLog;
using ReactiveUI;
using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.LogoutRequired)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LogoutRequiredViewModel : BaseViewModel, ILogoutRequiredViewModel
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
            CancelCommand = ReactiveCommand.Create();
        }

        public IReactiveCommand<ProgressState> Logout { get; }

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
            if (repositoryHosts.GitHubHost != null)
            {
                await repositoryHosts.GitHubHost.LogOut();
            }

            if (repositoryHosts.EnterpriseHost != null)
            {
                await repositoryHosts.EnterpriseHost.LogOut();
            }
        }
    }
}
