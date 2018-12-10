using System;
using System.ComponentModel.Composition;
using System.Reactive;
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

        [ImportingConstructor]
        public ForkRepositoryViewModel(
            IForkRepositorySelectViewModel selectPage,
            IForkRepositorySwitchViewModel switchPage,
            IForkRepositoryExecuteViewModel executePage)
        {
            this.selectPage = selectPage;
            this.executePage = executePage;
            this.switchPage = switchPage;

            Completed = ReactiveCommand.Create(() => { });

            //selectPage.SwitchOrigin.Subscribe(x => ShowSwitchRepositoryPath((IRemoteRepositoryModel)x));
            selectPage.Done.Subscribe(x => ShowExecutePage((IAccount)x).Forget());
            executePage.Back.Subscribe(x => ShowSelectPage().Forget());
        }

        public LocalRepositoryModel Repository { get; private set; }

        public IConnection Connection { get; private set; }

        private ReactiveCommand<Unit, Unit> Completed { get; }

        public override IObservable<object> Done => executePage.Done;

        public async Task InitializeAsync(LocalRepositoryModel repository, IConnection connection)
        {
            Repository = repository;
            Connection = connection;
            await ShowSelectPage().ConfigureAwait(false);
        }

        async Task ShowSelectPage()
        {
            await selectPage.InitializeAsync(Repository, Connection).ConfigureAwait(true);
            Content = selectPage;
        }

        async Task ShowExecutePage(IAccount account)
        {
            await executePage.InitializeAsync(Repository, account, Connection).ConfigureAwait(true);
            Content = executePage;
        }

        void ShowSwitchRepositoryPath(RemoteRepositoryModel remoteRepository)
        {
            switchPage.Initialize(Repository, remoteRepository);
            Content = switchPage;
        }
    }
}
