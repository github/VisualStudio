using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
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
        readonly IForkRepositoryExecuteViewModel executePage;
        ILocalRepositoryModel repository;
        IConnection connection;

        [ImportingConstructor]
        public ForkRepositoryViewModel(
            IForkRepositorySelectViewModel selectPage,
            IForkRepositoryExecuteViewModel executePage)
        {
            this.selectPage = selectPage;
            this.executePage = executePage;

            Completed = ReactiveCommand.Create();

            selectPage.CloneRepository.Subscribe(x => ShowCloneRepositoryPage((IRemoteRepositoryModel)x));
            selectPage.Done.Subscribe(x => ShowExecutePage((IAccount)x).Forget());
            executePage.Done.Subscribe(ExecuteForkOperation);
        }

        private ReactiveCommand<object> Completed { get; }

        public override IObservable<object> Done => Completed;

        public async Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection)
        {
            this.repository = repository;
            this.connection = connection;
            await selectPage.InitializeAsync(repository, connection);
            Content = selectPage;
        }

        async Task ShowExecutePage(IAccount account)
        {
            log.Verbose("ShowExecutePage {Login}", account.Login);

            await executePage.InitializeAsync(repository, connection, account);
            Content = executePage;
        }

        void ShowCloneRepositoryPage(IRemoteRepositoryModel remoteRepository)
        {
            log.Verbose("ShowCloneRepositoryPage {Owner}/{Name}", remoteRepository.Owner, remoteRepository.Name);
        }

        private void ExecuteForkOperation(object o)
        {
            log.Verbose("Fork {Source} to {Destination}", executePage.SourceRepository.ToString(), executePage.DestinationRepository.ToString());

            log.Verbose("Fork AddUpstream {AddUpstream}", executePage.AddUpstream);
            log.Verbose("Fork ResetMasterTracking {ResetMasterTracking}", executePage.ResetMasterTracking);
            log.Verbose("Fork UpdateOrigin {UpdateOrigin}", executePage.UpdateOrigin);

            Completed.Execute(o);
        }
    }
}
