using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.App;
using GitHub.Extensions.Reactive;
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
        IReadOnlyList<RemoteRepositoryModel> existingForks;
        bool isLoading;

        [ImportingConstructor]
        public ForkRepositorySelectViewModel(IModelServiceFactory modelServiceFactory)
        {
            this.modelServiceFactory = modelServiceFactory;
            SelectedAccount = ReactiveCommand.Create<IAccount, IAccount>(account => account);
            SwitchOrigin = ReactiveCommand.Create<RemoteRepositoryModel>(_ => { });
        }

        public string Title => Resources.ForkRepositoryTitle;

        public IReadOnlyList<IAccount> Accounts
        {
            get { return accounts; }
            private set { this.RaiseAndSetIfChanged(ref accounts, value); }
        }

        public IReadOnlyList<RemoteRepositoryModel> ExistingForks
        {
            get { return existingForks; }
            private set { this.RaiseAndSetIfChanged(ref existingForks, value); }
        }

        public bool IsLoading
        {
            get { return isLoading; }
            private set { this.RaiseAndSetIfChanged(ref isLoading, value); }
        }

        public ReactiveCommand<IAccount, IAccount> SelectedAccount { get; }

        public ReactiveCommand<RemoteRepositoryModel, Unit> SwitchOrigin { get; }

        public IObservable<object> Done => SelectedAccount;

        public async Task InitializeAsync(LocalRepositoryModel repository, IConnection connection)
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
                        var forks = x.Forks;

                        var parents = new List<RemoteRepositoryModel>();
                        var current = x.Respoitory;
                        while (current.Parent != null)
                        {
                            parents.Add(current.Parent);
                            current = current.Parent;
                        }

                        BuildAccounts(x.Accounts, repository, forks, parents);
                    });

            }
            catch (Exception ex)
            {
                log.Error(ex, "Error initializing ForkRepositoryViewModel");
                IsLoading = false;
            }
        }

        void BuildAccounts(IReadOnlyList<IAccount> accessibleAccounts, LocalRepositoryModel currentRepository, IList<RemoteRepositoryModel> forks, List<RemoteRepositoryModel> parents)
        {
            log.Verbose("BuildAccounts: {AccessibleAccounts} accessibleAccounts, {Forks} forks, {Parents} parents", accessibleAccounts.Count, forks.Count, parents.Count);

            var existingForksAndParents = forks.Union(parents).ToDictionary(model => model.Owner);

            var readOnlyList = accessibleAccounts
                .Where(account => account.Login != currentRepository.Owner)
                .Select(account => new { Account = account, Fork = existingForksAndParents.ContainsKey(account.Login) ? existingForksAndParents[account.Login] : null })
                .ToArray();

            Accounts = readOnlyList.Where(arg => arg.Fork == null).Select(arg => arg.Account).ToList();
            ExistingForks = readOnlyList.Where(arg => arg.Fork != null).Select(arg => arg.Fork).ToList();

            // HACK: Our avatar cache only provides avatars in a very small size, but we want to 
            // display them 100x100 in the Fork view. For now, wse the AvatarUrl directly to get
            // the avatar, appending "s=100" to the URL to get the correct size.
            foreach (Account account in Accounts)
            {
                account.AvatarUrl += "&s=100";
            }
        }
    }
}
