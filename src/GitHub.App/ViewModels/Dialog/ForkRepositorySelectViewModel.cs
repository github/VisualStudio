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
                    modelService.GetForks(repository).ToList(),
                    (a, r, f) => new { Accounts = a, Respoitory = r, Forks = f })
                    .Finally(() => IsLoading = false)
                    .Subscribe(x =>
                    {
                        var forksAndParents = new List<IRemoteRepositoryModel>(x.Forks);
                        var current = x.Respoitory;
                        while (current.Parent != null)
                        {
                            forksAndParents.Add(current.Parent);
                            current = current.Parent;
                        }

                        Accounts = BuildAccounts(x.Accounts, forksAndParents, repository.Owner);
                        ExistingForks = forksAndParents;

                        log.Verbose("Loaded Data Accounts:{Accounts} Forks:{Forks}", Accounts.Count, ExistingForks.Count);
                    });

            }
            catch (Exception ex)
            {
                log.Error(ex, "Error initializing ForkRepositoryViewModel");
                IsLoading = false;
            }
        }

        IReadOnlyList<IAccount> BuildAccounts(IEnumerable<IAccount> accessibleAccounts, IList<IRemoteRepositoryModel> forksAndParents, string currentRepositoryOwner)
        {
            var forksByOwner = forksAndParents.ToDictionary(x => x.Owner, x => x);
            return accessibleAccounts
                .Where(x => x.Login != currentRepositoryOwner)
                .Where(x => !forksByOwner.ContainsKey(x.Login)).ToList();
        }
    }
}
