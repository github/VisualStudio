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
        readonly IForkRepositorySelectViewModel selectPage;
        readonly IForkRepositorySwitchViewModel switchPage;
        readonly IForkRepositoryExecuteViewModel executePage;
        readonly IRepositoryForkService repositoryForkService;

        [ImportingConstructor]
        public ForkRepositoryViewModel(
            IForkRepositorySelectViewModel selectPage,
            IForkRepositorySwitchViewModel switchPage,
            IForkRepositoryExecuteViewModel executePage,
            IRepositoryForkService repositoryForkService)
        {
            this.selectPage = selectPage;
            this.executePage = executePage;
            this.repositoryForkService = repositoryForkService;
            this.switchPage = switchPage;

            Completed = ReactiveCommand.Create();

            selectPage.SwitchOrigin.Subscribe(x => ShowSwitchRepositoryPath((IRemoteRepositoryModel)x));
            selectPage.Done.Subscribe(x => ShowExecutePage((IAccount)x).Forget());
        }

        public ILocalRepositoryModel Repository { get; private set; }

        public IConnection Connection { get; private set; }

        private ReactiveCommand<object> Completed { get; }

        public override IObservable<object> Done => executePage.Done;

        public async Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection)
        {
            Repository = repository;
            Connection = connection;
            await selectPage.InitializeAsync(repository, connection);
            Content = selectPage;
        }

        async Task ShowExecutePage(IAccount account)
        {
            await executePage.InitializeAsync(Repository, account, Connection);
            Content = executePage;
        }

        void ShowSwitchRepositoryPath(IRemoteRepositoryModel remoteRepository)
        {
            switchPage.Initialize(Repository, remoteRepository);
            Content = switchPage;
        }
    }
}
