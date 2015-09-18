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

        readonly IRepositoryHosts hosts;
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
            IVSServices vsServices,
            IConnectionManager connectionManager)
        {
            this.vsServices = vsServices;
            this.hosts = hosts;

            title = this.WhenAny(
                x => x.SelectedHost,
                x => x.Value != null ?
                    string.Format(CultureInfo.CurrentCulture, Resources.PublishToTitle, x.Value.Title) :
                    Resources.PublishTitle
            )
            .ToProperty(this, x => x.Title);

            Connections = new ReactiveList<IConnection>(connectionManager.Connections);
            this.repositoryPublishService = repositoryPublishService;

            if (Connections.Any())
            {
                SelectedConnection = Connections.FirstOrDefault(x => x.HostAddress.IsGitHubDotCom()) ?? Connections[0];
            }

            accounts = this.WhenAny(x => x.SelectedConnection, x => x.Value != null ? hosts.LookupHost(x.Value.HostAddress) : RepositoryHosts.DisconnectedRepositoryHost)
                .Where(x => !(x is DisconnectedRepositoryHost))
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

            isHostComboBoxVisible = this.WhenAny(x => x.Connections, x => x.Value)
                .WhereNotNull()
                .Select(h => h.Count > 1)
                .ToProperty(this, x => x.IsHostComboBoxVisible);

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

        public string DefaultRepositoryName { get; private set; }
        public new string Title { get { return title.Value; } }
        public bool CanKeepPrivate { get { return canKeepPrivate.Value; } }
        public bool IsPublishing { get { return isPublishing.Value; } }

        public IReactiveCommand<Unit> PublishRepository { get; private set; }
        public ReactiveList<IConnection> Connections { get; private set; }

        IConnection selectedConnection;
        [AllowNull]
        public IConnection SelectedConnection
        {
            [return: AllowNull]
            get { return selectedConnection; }
            set { this.RaiseAndSetIfChanged(ref selectedConnection, value); }
        }

        IRepositoryHost SelectedHost
        {
            [return:AllowNull]
            get { return selectedConnection != null ? hosts.LookupHost(selectedConnection.HostAddress) : null; }
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
                .IfNullOrEmpty(Resources.RepositoryNameValidatorEmpty)
                .IfTrue(x => x.Length > 100, Resources.RepositoryNameValidatorTooLong);

            SafeRepositoryNameWarningValidator = ReactivePropertyValidator.ForObservable(nonNullRepositoryName)
                .Add(repoName =>
                {
                    var parsedReference = GetSafeRepositoryName(repoName);
                    return parsedReference != repoName ? String.Format(CultureInfo.CurrentCulture, Resources.SafeRepositoryNameWarning, parsedReference) : null;
                });

            this.WhenAny(x => x.SafeRepositoryNameWarningValidator.ValidationResult, x => x.Value)
                .WhereNotNull() // When this is instantiated, it sends a null result.
                .Select(result => result?.Message)
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
