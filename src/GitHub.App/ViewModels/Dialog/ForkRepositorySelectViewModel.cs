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
        static readonly ILogger log = LogManager.ForContext<ForkRepositoryViewModel>();

        readonly IModelServiceFactory modelServiceFactory;
        IReadOnlyList<IAccount> accounts;
        IReadOnlyList<IRemoteRepositoryModel> existingForks;
        bool isLoading;

        [ImportingConstructor]
        public ForkRepositorySelectViewModel(IModelServiceFactory modelServiceFactory)
        {
            this.modelServiceFactory = modelServiceFactory;
            Selected = ReactiveCommand.Create();
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

        public ReactiveCommand<object> Selected { get; }

        public IObservable<object> Done => Selected;

        public async Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection)
        {
            IsLoading = true;

            try
            {
                var modelService = await modelServiceFactory.CreateAsync(connection);

                Observable.CombineLatest(
                    modelService.GetAccounts(),
                    modelService.GetForks(repository).ToList(),
                    (a, f) => new { Accounts = a, Forks = f })
                    .Finally(() => IsLoading = false)
                    .Subscribe(x =>
                    {
                        Accounts = BuildAccounts(x.Accounts, x.Forks);
                        ExistingForks = BuildExistingForks(x.Accounts, x.Forks);
                    });

            }
            catch (Exception ex)
            {
                log.Error(ex, "Error initializing ForkRepositoryViewModel");
                IsLoading = false;
            }
        }

        IReadOnlyList<IAccount> BuildAccounts(IReadOnlyList<IAccount> accounts, IList<IRemoteRepositoryModel> forks)
        {
            var forksByOwner = forks.ToDictionary(x => x.Owner, x => x);
            return accounts.Where(x => !forksByOwner.ContainsKey(x.Login)).ToList();
        }

        IReadOnlyList<IRemoteRepositoryModel> BuildExistingForks(IReadOnlyList<IAccount> accounts, IList<IRemoteRepositoryModel> forks)
        {
            var accountsByLogin = accounts.ToDictionary(x => x.Login, x => x);
            return forks.Where(x => accountsByLogin.ContainsKey(x.Owner)).ToList();
        }
    }
}
