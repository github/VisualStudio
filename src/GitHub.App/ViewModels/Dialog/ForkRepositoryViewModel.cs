using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;
using Serilog;

namespace GitHub.ViewModels.Dialog
{
    [Export(typeof(IForkRepositoryViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ForkRepositoryViewModel : PagedDialogViewModelBase, IForkRepositoryViewModel
    {
        static readonly ILogger log = LogManager.ForContext<ForkRepositoryViewModel>();

        readonly IForkRepositorySelectViewModel selectPage;
        readonly IForkRepositorySwitchViewModel switchPage;
        readonly IForkRepositoryExecuteViewModel executePage;

        ILocalRepositoryModel repository;
        IConnection connection;

        [ImportingConstructor]
        public ForkRepositoryViewModel(
            IForkRepositorySelectViewModel selectPage,
            IForkRepositorySwitchViewModel switchPage,
            IForkRepositoryExecuteViewModel executePage)
        {
            this.selectPage = selectPage;
            this.executePage = executePage;
            this.switchPage = switchPage;

            Completed = ReactiveCommand.Create();

            selectPage.SwitchOrigin.Subscribe(x => ShowSwitchRepositoryPath((IRemoteRepositoryModel)x));
            selectPage.Done.Subscribe(x => ShowExecutePage((IAccount)x).Forget());
        }

        private ReactiveCommand<object> Completed { get; }

        public override IObservable<object> Done => Observable.SelectMany(executePage.Done, switchPage.Done);

        public async Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection)
        {
            this.repository = repository;
            this.connection = connection;
            await selectPage.InitializeAsync(repository, connection);
            Content = selectPage;
        }

        async Task ShowExecutePage(IAccount account)
        {
            await executePage.InitializeAsync(repository, account, connection);
            Content = executePage;
        }

        void ShowSwitchRepositoryPath(IRemoteRepositoryModel remoteRepository)
        {
            switchPage.Initialize(repository, remoteRepository);
            Content = switchPage;
        }
    }
}
