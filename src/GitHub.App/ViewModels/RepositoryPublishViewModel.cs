using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Exports;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using GitHub.Services;
using GitHub.UserErrors;
using GitHub.Validation;
using NLog;
using NullGuard;
using ReactiveUI;
using GitHub.Extensions;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.Publish)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryPublishViewModel : RepositoryFormViewModel, IRepositoryPublishViewModel
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly ObservableAsPropertyHelper<IReadOnlyList<IAccount>> accounts;
        readonly IRepositoryPublishService repositoryPublishService;
        readonly ObservableAsPropertyHelper<bool> canKeepPrivate;
        readonly ObservableAsPropertyHelper<bool> isPublishing;
        readonly ObservableAsPropertyHelper<string> title;

        [ImportingConstructor]
        public RepositoryPublishViewModel(
            IRepositoryHosts hosts,
            IAvatarProvider avatarProvider,
            IRepositoryPublishService repositoryPublishService)
        {
            title = this.WhenAny(
                x => x.SelectedHost,
                x => x.Value != null ?
                    string.Format(CultureInfo.CurrentCulture, "Publish repository to {0}", x.Value.Title) :
                    "Publish repository"
            )
            .ToProperty(this, x => x.Title);

            RepositoryHosts = new ReactiveList<IRepositoryHost>(
                new[] { hosts.GitHubHost, hosts.EnterpriseHost }.Where(h => h.IsLoggedIn));
            this.repositoryPublishService = repositoryPublishService;

            if (RepositoryHosts.Any())
            {
                SelectedHost = RepositoryHosts[0];
            }

            var accountsChangedObservable = this.WhenAny(x => x.SelectedHost, x => x.Value)
                .WhereNotNull()
                .SelectMany(host => host.GetAccounts(avatarProvider));

            accounts = accountsChangedObservable
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.Accounts, initialValue: new ReadOnlyCollection<IAccount>(new IAccount[] {}));

            accountsChangedObservable
                .Where(acts => acts.Any())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(acts => SelectedAccount = acts[0]);

            var nonNullRepositoryName = this.WhenAny(
                x => x.RepositoryName,
                x => x.Value)
                .WhereNotNull();

            RepositoryNameValidator = ReactivePropertyValidator.ForObservable(nonNullRepositoryName)
                .IfNullOrEmpty("Please enter a repository name")
                .IfTrue(x => x.Length > 100, "Repository name must be fewer than 100 characters");

            SafeRepositoryNameWarningValidator = ReactivePropertyValidator.ForObservable(nonNullRepositoryName)
                .Add(repoName =>
                {
                    var parsedReference = GetSafeRepositoryName(repoName);
                    return parsedReference != repoName ? "Will be created as " + parsedReference : null;
                });

            PublishRepository = InitializePublishRepositoryCommand();

            canKeepPrivate = CanKeepPrivateObservable.CombineLatest(PublishRepository.IsExecuting,
                (canKeep, publishing) => canKeep && !publishing)
                .ToProperty(this, x => x.CanKeepPrivate);

            isPublishing = PublishRepository.IsExecuting
                .ToProperty(this, x => x.IsPublishing);
        }

        public new string Title { get { return title.Value; } }
        public bool CanKeepPrivate { get { return canKeepPrivate.Value; } }
        public bool IsPublishing { get { return isPublishing.Value; } }

        public IReactiveCommand<Unit> PublishRepository { get; private set; }
        public ReactiveList<IRepositoryHost> RepositoryHosts { get; private set; }

        IRepositoryHost selectedHost;
        [AllowNull]
        public IRepositoryHost SelectedHost
        {
            [return: AllowNull]
            get { return selectedHost; }
            set { this.RaiseAndSetIfChanged(ref selectedHost, value); }
        }

        public IReadOnlyList<IAccount> Accounts
        {
            get { return accounts.Value; }
        }

        ReactiveCommand<Unit> InitializePublishRepositoryCommand()
        {
            var canCreate = this.WhenAny(x => x.RepositoryNameValidator.ValidationResult.IsValid, x => x.Value);
            var publishCommand = ReactiveCommand.CreateAsyncObservable(canCreate, OnPublishRepository);
            publishCommand.ThrownExceptions.Subscribe(ex =>
            {
                if (!ex.IsCriticalException())
                {
                    // TODO: Throw a proper error.
                    log.Error("Error creating repository.", ex);
                    UserError.Throw(new PublishRepositoryUserError(ex.Message));
                }
            });

            return publishCommand;
        }

        private IObservable<Unit> OnPublishRepository(object arg)
        {
            var newRepository = GatherRepositoryInfo();
            var account = SelectedAccount;

            return repositoryPublishService.PublishRepository(newRepository, account, SelectedHost.ApiClient)
                .SelectUnit();
        }
    }
}
