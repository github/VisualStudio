using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using GitHub.Services;
using GitHub.UserErrors;
using GitHub.Validation;
using NLog;
using NullGuard;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.Publish)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryPublishViewModel : RepositoryFormViewModel, IRepositoryPublishViewModel
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly IRepositoryPublishService repositoryPublishService;
        readonly IVSServices vsServices;
        readonly ObservableAsPropertyHelper<IReadOnlyList<IAccount>> accounts;
        readonly ObservableAsPropertyHelper<bool> isHostComboBoxVisible;
        readonly ObservableAsPropertyHelper<bool> canKeepPrivate;
        readonly ObservableAsPropertyHelper<bool> isPublishing;
        readonly ObservableAsPropertyHelper<string> title;
        
        [ImportingConstructor]
        public RepositoryPublishViewModel(
            IRepositoryHosts hosts,
            IRepositoryPublishService repositoryPublishService,
            IVSServices vsServices)
        {
            this.vsServices = vsServices;

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

            accounts = this.WhenAny(x => x.SelectedHost, x => x.Value)
                .WhereNotNull()
                .SelectMany(host => host.ModelService.GetAccounts())
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.Accounts, initialValue: new ReadOnlyCollection<IAccount>(new IAccount[] {}));

            this.WhenAny(x => x.Accounts, x => x.Value)
                .WhereNotNull()
                .Where(accts => accts.Any())
                .Subscribe(accts => {
                    var selectedAccount = accts.FirstOrDefault();
                    if (selectedAccount != null)
                    {
                        SelectedAccount = accts.FirstOrDefault();
                    }
                });

            isHostComboBoxVisible = this.WhenAny(x => x.RepositoryHosts, x => x.Value)
                .WhereNotNull()
                .Select(h => h.Count > 1)
                .ToProperty(this, x => x.IsHostComboBoxVisible, initialValue: false);

            InitializeValidation();

            PublishRepository = InitializePublishRepositoryCommand();

            canKeepPrivate = CanKeepPrivateObservable.CombineLatest(PublishRepository.IsExecuting,
                (canKeep, publishing) => canKeep && !publishing)
                .ToProperty(this, x => x.CanKeepPrivate);

            isPublishing = PublishRepository.IsExecuting
                .ToProperty(this, x => x.IsPublishing);

            var defaultRepositoryName = repositoryPublishService.LocalRepositoryName;
            if (!string.IsNullOrEmpty(defaultRepositoryName))
            {
                DefaultRepositoryName    = defaultRepositoryName;
            }
        }

        public string DefaultRepositoryName { get; private set; }
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

        public bool IsHostComboBoxVisible
        {
            get { return isHostComboBoxVisible.Value; }
        }

        ReactiveCommand<Unit> InitializePublishRepositoryCommand()
        {
            var canCreate = this.WhenAny(x => x.RepositoryNameValidator.ValidationResult.IsValid, x => x.Value);
            return ReactiveCommand.CreateAsyncObservable(canCreate, OnPublishRepository);
        }

        private IObservable<Unit> OnPublishRepository(object arg)
        {
            var newRepository = GatherRepositoryInfo();
            var account = SelectedAccount;

            return repositoryPublishService.PublishRepository(newRepository, account, SelectedHost.ApiClient)
                .SelectUnit()
                .Do(_ => vsServices.ShowMessage("Repository published successfully."))
                .Catch<Unit, Exception>(ex =>
                {
                    if (!ex.IsCriticalException())
                    {
                        log.Error(ex);
                        var error = new PublishRepositoryUserError(ex.Message);
                        vsServices.ShowError((error.ErrorMessage + Environment.NewLine + error.ErrorCauseOrResolution).TrimEnd());
                    }
                    return Observable.Return(Unit.Default);
                });
        }

        void InitializeValidation()
        {
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

            this.WhenAny(x => x.SafeRepositoryNameWarningValidator.ValidationResult, x => x.Value)
                .WhereNotNull() // When this is instantiated, it sends a null result.
                .Select(result => result == null ? null : result.Message)
                .Subscribe(message =>
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        vsServices.ShowWarning(message);
                    }
                    else
                    {
                        vsServices.ClearNotifications();
                    }
                });
        }
    }
}
