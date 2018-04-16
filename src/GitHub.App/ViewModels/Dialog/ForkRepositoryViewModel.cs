using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;

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
            selectPage.Done.Subscribe(x => ShowExecutePage((IAccount)x).Forget());
        }

        public override IObservable<object> Done => null;

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
    }
}
