using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.App;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Factories;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using GitHub.UserErrors;
using GitHub.Validation;
using ReactiveUI;
using Serilog;

namespace GitHub.ViewModels.TeamExplorer
{
    [Export(typeof(IRepositoryPublishViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryPublishViewModel : RepositoryFormViewModel, IRepositoryPublishViewModel
    {
        static readonly ILogger log = LogManager.ForContext<RepositoryPublishViewModel>();

        readonly IRepositoryPublishService repositoryPublishService;
        readonly INotificationService notificationService;
        readonly IModelServiceFactory modelServiceFactory;
        readonly ObservableAsPropertyHelper<IReadOnlyList<IAccount>> accounts;
        readonly ObservableAsPropertyHelper<bool> isHostComboBoxVisible;
        readonly ObservableAsPropertyHelper<string> title;
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        public RepositoryPublishViewModel(
            IRepositoryPublishService repositoryPublishService,
            INotificationService notificationService,
            IConnectionManager connectionManager,
            IModelServiceFactory modelServiceFactory,
            IUsageTracker usageTracker)
        {
            Guard.ArgumentNotNull(repositoryPublishService, nameof(repositoryPublishService));
            Guard.ArgumentNotNull(notificationService, nameof(notificationService));
            Guard.ArgumentNotNull(connectionManager, nameof(connectionManager));
            Guard.ArgumentNotNull(usageTracker, nameof(usageTracker));
            Guard.ArgumentNotNull(modelServiceFactory, nameof(modelServiceFactory));

            this.notificationService = notificationService;
            this.usageTracker = usageTracker;
            this.modelServiceFactory = modelServiceFactory;

            title = this.WhenAny(
                x => x.SelectedConnection,
                x => x.Value != null ?
                    string.Format(CultureInfo.CurrentCulture, Resources.PublishToTitle, x.Value.HostAddress.Title) :
                    Resources.PublishTitle
            )
            .ToProperty(this, x => x.Title);

            Connections = connectionManager.Connections;
            this.repositoryPublishService = repositoryPublishService;

            if (Connections.Any())
                SelectedConnection = Connections.FirstOrDefault(x => x.HostAddress.IsGitHubDotCom()) ?? Connections[0];

            accounts = this.WhenAnyValue(x => x.SelectedConnection)
                .Where(x => x != null)
                .SelectMany(async c => (await modelServiceFactory.CreateAsync(c)).GetAccounts())
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.Accounts, initialValue: new ReadOnlyCollection<IAccount>(Array.Empty<IAccount>()));

            this.WhenAny(x => x.Accounts, x => x.Value)
                .WhereNotNull()
                .Where(accts => accts.Any())
                .Subscribe(accts => {
                    var selectedAccount = accts.FirstOrDefault();
                    if (selectedAccount != null)
                        SelectedAccount = accts.FirstOrDefault();
                });

            isHostComboBoxVisible = this.WhenAny(x => x.Connections, x => x.Value)
                .WhereNotNull()
                .Select(h => h.Count > 1)
                .ToProperty(this, x => x.IsHostComboBoxVisible);

            InitializeValidation();

            PublishRepository = InitializePublishRepositoryCommand();
            PublishRepository.IsExecuting.Subscribe(x => IsBusy = x);

            var defaultRepositoryName = repositoryPublishService.LocalRepositoryName;
            if (!string.IsNullOrEmpty(defaultRepositoryName))
                RepositoryName = defaultRepositoryName;

            this.WhenAny(x => x.SelectedConnection, x => x.SelectedAccount,
                (a,b) => true)
                .Where(x => RepositoryNameValidator.ValidationResult != null && SafeRepositoryNameWarningValidator.ValidationResult != null)
                .Subscribe(async _ =>
                {
                    var name = RepositoryName;
                    RepositoryName = null;
                    await RepositoryNameValidator.ResetAsync();
                    await SafeRepositoryNameWarningValidator.ResetAsync();
                    RepositoryName = name;
                });
        }

        public string Title { get { return title.Value; } }

        public ReactiveCommand<Unit, ProgressState> PublishRepository { get; private set; }
        public IReadOnlyObservableCollection<IConnection> Connections { get; private set; }

        bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            private set { this.RaiseAndSetIfChanged(ref isBusy, value); }
        }

        IConnection selectedConnection;
        public IConnection SelectedConnection
        {
            get { return selectedConnection; }
            set { this.RaiseAndSetIfChanged(ref selectedConnection, value); }
        }

        public IReadOnlyList<IAccount> Accounts
        {
            get { return accounts.Value; }
        }

        public bool IsHostComboBoxVisible
        {
            get { return isHostComboBoxVisible.Value; }
        }

        ReactiveCommand<Unit, ProgressState> InitializePublishRepositoryCommand()
        {
            var canCreate = this.WhenAny(x => x.RepositoryNameValidator.ValidationResult.IsValid, x => x.Value);
            return ReactiveCommand.CreateFromObservable(OnPublishRepository, canCreate);
        }

        IObservable<ProgressState> OnPublishRepository()
        {
            var newRepository = GatherRepositoryInfo();
            var account = SelectedAccount;
            var modelService = modelServiceFactory.CreateBlocking(SelectedConnection);

            return repositoryPublishService.PublishRepository(newRepository, account, modelService.ApiClient)
                .Do(_ => usageTracker.IncrementCounter(x => x.NumberOfReposPublished).Forget())
                .Select(_ => ProgressState.Success)
                .Catch<ProgressState, Exception>(ex =>
                {
                    if (!ex.IsCriticalException())
                    {
                        log.Error(ex, "Error Publishing Repository");
                        var error = new PublishRepositoryUserError(ex.Message);
                        notificationService.ShowError((error.ErrorMessage + Environment.NewLine + error.ErrorCauseOrResolution).TrimEnd());
                    }
                    return Observable.Return(ProgressState.Fail);
                });
        }

        void InitializeValidation()
        {
            var nonNullRepositoryName = this.WhenAny(
                x => x.RepositoryName,
                x => x.Value)
                .WhereNotNull();

            RepositoryNameValidator = ReactivePropertyValidator.ForObservable(nonNullRepositoryName)
                .IfNullOrEmpty(Resources.RepositoryNameValidatorEmpty)
                .IfTrue(x => x.Length > 100, Resources.RepositoryNameValidatorTooLong);

            SafeRepositoryNameWarningValidator = ReactivePropertyValidator.ForObservable(nonNullRepositoryName)
                .Add(repoName =>
                {
                    var parsedReference = GetSafeRepositoryName(repoName);
                    return parsedReference != repoName ? String.Format(CultureInfo.CurrentCulture, Resources.SafeRepositoryNameWarning, parsedReference) : null;
                });
        }
    }
}
