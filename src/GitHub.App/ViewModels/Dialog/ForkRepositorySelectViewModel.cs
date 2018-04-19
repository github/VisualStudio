using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.App;
using GitHub.Factories;
using GitHub.Logging;
using GitHub.Models;
using ReactiveUI;
using Serilog;

namespace GitHub.ViewModels.Dialog
{
    [Export(typeof(IForkRepositorySelectViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ForkRepositorySelectViewModel : ViewModelBase, IForkRepositorySelectViewModel
    {
        static readonly ILogger log = LogManager.ForContext<ForkRepositorySelectViewModel>();

        readonly IModelServiceFactory modelServiceFactory;
        IReadOnlyList<IAccount> accounts;
        IReadOnlyList<IRemoteRepositoryModel> existingForks;
        bool isLoading;

        [ImportingConstructor]
        public ForkRepositorySelectViewModel(IModelServiceFactory modelServiceFactory)
        {
            this.modelServiceFactory = modelServiceFactory;
            SelectedAccount = ReactiveCommand.Create();
            SwitchOrigin = ReactiveCommand.Create();
        }

        public string Title => Resources.ForkRepositoryTitle;

        public IReadOnlyList<IAccount> Accounts
        {
            get { return accounts; }
            private set { this.RaiseAndSetIfChanged(ref accounts, value); }
        }

        public IReadOnlyList<IRemoteRepositoryModel> ExistingForks
        {
            get { return existingForks; }
            private set { this.RaiseAndSetIfChanged(ref existingForks, value); }
        }

        public bool IsLoading
        {
            get { return isLoading; }
            private set { this.RaiseAndSetIfChanged(ref isLoading, value); }
        }

        public ReactiveCommand<object> SelectedAccount { get; }

        public ReactiveCommand<object> SwitchOrigin { get; }

        public IObservable<object> Done => SelectedAccount;

        public async Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection)
        {
            IsLoading = true;

            try
            {
                var modelService = await modelServiceFactory.CreateAsync(connection);

                Observable.CombineLatest(
                    modelService.GetAccounts(),
                    modelService.GetRepository(repository.Owner, repository.Name),
                    (a, r) => new { Accounts = a, Respoitory = r})
                    .Finally(() => IsLoading = false)
                    .Subscribe(x =>
                    {
                        Accounts = BuildAccounts(x.Accounts, repository.Owner);
                    });

            }
            catch (Exception ex)
            {
                log.Error(ex, "Error initializing ForkRepositoryViewModel");
                IsLoading = false;
            }
        }

        IReadOnlyList<IAccount> BuildAccounts(IEnumerable<IAccount> accessibleAccounts, string currentRepositoryOwner)
        {
            return accessibleAccounts
                .Where(x => x.Login != currentRepositoryOwner).ToList();
        }
    }
}
