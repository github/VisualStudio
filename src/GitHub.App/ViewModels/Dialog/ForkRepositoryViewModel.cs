using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    [Export(typeof(IForkRepositoryViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ForkRepositoryViewModel : PagedDialogViewModelBase, IForkRepositoryViewModel
    {
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
            await executePage.InitializeAsync(repository, connection, account);
            Content = executePage;
        }

        void ShowCloneRepositoryPage(IRemoteRepositoryModel remoteRepository)
        {
        }

        private void ExecuteForkOperation(object o)
        {
            Completed.Execute(o);
        }
    }
}
