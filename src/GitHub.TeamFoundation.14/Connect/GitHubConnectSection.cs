using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.Settings;
using GitHub.UI;
using GitHub.UserErrors;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using GitHub.VisualStudio.UI;
using GitHub.VisualStudio.UI.Views;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio;
using ReactiveUI;
using Serilog;

namespace GitHub.VisualStudio.TeamExplorer.Connect
{
    public class GitHubConnectSection : TeamExplorerSectionBase, IGitHubConnectSection
    {
        static readonly ILogger log = LogManager.ForContext<GitHubConnectSection>();
        readonly IPackageSettings packageSettings;
        readonly IVSServices vsServices;
        readonly int sectionIndex;
        readonly ILocalRepositories localRepositories;
        readonly IUsageTracker usageTracker;
        GitHubConnectSectionState settings;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        SectionStateTracker sectionTracker;

        protected GitHubConnectContent View
        {
            get { return SectionContent as GitHubConnectContent; }
            set { SectionContent = value; }
        }

        public IConnection SectionConnection { get; set; }

        bool loggedIn;
        bool LoggedIn
        {
            get { return loggedIn; }
            set
            {
                loggedIn = ShowLogout = value;
                ShowLogin = !value;
            }
        }

        bool showLogin;
        public bool ShowLogin
        {
            get { return showLogin; }
            set { showLogin = value; this.RaisePropertyChange(); }
        }

        bool showLogout;
        public bool ShowLogout
        {
            get { return showLogout; }
            set { showLogout = value; this.RaisePropertyChange(); }
        }

        IReactiveDerivedList<ILocalRepositoryModel> repositories;
        public IReactiveDerivedList<ILocalRepositoryModel> Repositories
        {
            get { return repositories; }
            set { repositories = value; this.RaisePropertyChange(); }
        }

        ILocalRepositoryModel selectedRepository;
        public ILocalRepositoryModel SelectedRepository
        {
            get { return selectedRepository; }
            set { selectedRepository = value; this.RaisePropertyChange(); }
        }

        public ICommand Clone { get; }

        internal ITeamExplorerServiceHolder Holder => holder;

        public GitHubConnectSection(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory,
            ITeamExplorerServiceHolder holder,
            IConnectionManager manager,
            IPackageSettings packageSettings,
            IVSServices vsServices,
            ILocalRepositories localRepositories,
            IUsageTracker usageTracker,
            int index)
            : base(serviceProvider, apiFactory, holder, manager)
        {
            Guard.ArgumentNotNull(apiFactory, nameof(apiFactory));
            Guard.ArgumentNotNull(holder, nameof(holder));
            Guard.ArgumentNotNull(manager, nameof(manager));
            Guard.ArgumentNotNull(packageSettings, nameof(packageSettings));
            Guard.ArgumentNotNull(vsServices, nameof(vsServices));
            Guard.ArgumentNotNull(localRepositories, nameof(localRepositories));
            Guard.ArgumentNotNull(usageTracker, nameof(usageTracker));

            Title = "GitHub";
            IsEnabled = true;
            IsVisible = false;
            LoggedIn = false;
            sectionIndex = index;

            this.packageSettings = packageSettings;
            this.vsServices = vsServices;
            this.localRepositories = localRepositories;
            this.usageTracker = usageTracker;

            Clone = CreateAsyncCommandHack(DoClone);

            connectionManager.Connections.CollectionChanged += RefreshConnections;
            PropertyChanged += OnPropertyChange;
            UpdateConnection();
        }

        async Task DoClone()
        {
            var dialogService = ServiceProvider.GetService<IDialogService>();
            var result = await dialogService.ShowCloneDialog(SectionConnection);

            if (result != null)
            {
                try
                {
                    ServiceProvider.GitServiceProvider = TEServiceProvider;
                    var cloneService = ServiceProvider.GetService<IRepositoryCloneService>();
                    var repo = await cloneService.CloneRepository(
                        result.Repository.CloneUrl,
                        result.Repository.Name,
                        result.BasePath);
                    await HandleClonedRepo(repo);
                    usageTracker.IncrementCounter(x => x.NumberOfGitHubConnectSectionClones).Forget();
                }
                catch (Exception e)
                {
                    var teServices = ServiceProvider.TryGetService<ITeamExplorerServices>();
                    teServices.ShowError(e.GetUserFriendlyErrorMessage(ErrorType.ClonedFailed, result.Repository.Name));
                }
            }
        }

        void RefreshConnections(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (connectionManager.Connections.Count > sectionIndex)
                        Refresh(connectionManager.Connections[sectionIndex]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Refresh(connectionManager.Connections.Count <= sectionIndex
                        ? null
                        : connectionManager.Connections[sectionIndex]);
                    break;
            }
        }

        protected void Refresh(IConnection connection)
        {
            if (connection == null || !connection.IsLoggedIn)
            {
                LoggedIn = false;
                IsVisible = false;
                SectionConnection = null;
                if (Repositories != null)
                    Repositories.CollectionChanged -= UpdateRepositoryList;
                Repositories = null;
                settings = null;

                if (sectionIndex == 0 && TEServiceProvider != null)
                {
                    var section = GetSection(TeamExplorerInvitationBase.TeamExplorerInvitationSectionGuid);
                    IsVisible = !(section?.IsVisible ?? true); // only show this when the invitation section is hidden. When in doubt, don't show it.
                    if (section != null)
                        section.PropertyChanged += (s, p) =>
                        {
                            if (p.PropertyName == "IsVisible")
                                IsVisible = LoggedIn || !((ITeamExplorerSection)s).IsVisible;
                        };
                }
            }
            else
            {
                if (connection != SectionConnection)
                {
                    SectionConnection = connection;
                    Repositories?.Dispose();
                    Repositories = localRepositories.GetRepositoriesForAddress(connection.HostAddress);
                    Repositories.CollectionChanged += UpdateRepositoryList;
                    Title = connection.HostAddress.Title;
                    IsVisible = true;
                    LoggedIn = true;
                    settings = packageSettings.UIState.GetOrCreateConnectSection(Title);
                    IsExpanded = settings.IsExpanded;
                }
                if (TEServiceProvider != null)
                    RefreshRepositories().Forget();
            }
        }

        public override void Refresh()
        {
            UpdateConnection();
            base.Refresh();
        }

        public override void Initialize(IServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

            base.Initialize(serviceProvider);
            UpdateConnection();

            // watch for new repos added to the local repo list
            var section = GetSection(TeamExplorerConnectionsSectionId);
            if (section != null)
                sectionTracker = new SectionStateTracker(section, RefreshRepositories);
        }

        void UpdateConnection()
        {
            Refresh(connectionManager.Connections.Count > sectionIndex
                ? connectionManager.Connections[sectionIndex]
                : SectionConnection);
        }

        void OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsVisible" && IsVisible && View == null)
                View = new GitHubConnectContent { DataContext = this };
            else if (e.PropertyName == "IsExpanded" && settings != null)
                settings.IsExpanded = IsExpanded;
        }

        void UpdateRepositoryList(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                e.NewItems
                    .Cast<ILocalRepositoryModel>()
                    .ForEach(async r =>
                {
                    if (Equals(Holder.ActiveRepo, r))
                        SelectedRepository = r;

                    try
                    {
                        // TODO: Cache the icon state.
                        var api = await ApiFactory.Create(r.CloneUrl);
                        var repo = await api.GetRepository();
                        r.SetIcon(repo.Private, repo.Fork);
                    }
                    catch (Exception ex)
                    {
                        // GetRepository() may throw if the user doesn't have permissions to access the repo
                        // (because the repo no longer exists, or because the user has logged in on a different
                        // profile, or their permissions have changed remotely)
                        log.Error(ex, "Error updating repository list");
                    }
                });
            }
        }

        async Task HandleCreatedRepo(ILocalRepositoryModel newrepo)
        {
            Guard.ArgumentNotNull(newrepo, nameof(newrepo));

            var msg = string.Format(CultureInfo.CurrentCulture, Constants.Notification_RepoCreated, newrepo.Name, newrepo.CloneUrl);
            msg += " " + string.Format(CultureInfo.CurrentCulture, Constants.Notification_CreateNewProject, newrepo.LocalPath);

            // The creation involves a clone, and the clone will show a notification that we don't want.
            // Put a small pause in here to make sure that it's appeared so we can clear it.
            await Task.Delay(100);

            ShowNotification(newrepo, msg);
        }

        async Task HandleClonedRepo(ILocalRepositoryModel newrepo)
        {
            Guard.ArgumentNotNull(newrepo, nameof(newrepo));

            var msg = string.Format(CultureInfo.CurrentCulture, Constants.Notification_RepoCloned, newrepo.Name, newrepo.CloneUrl);
            if (newrepo.HasCommits() && newrepo.MightContainSolution())
                msg += " " + string.Format(CultureInfo.CurrentCulture, Constants.Notification_OpenProject, newrepo.LocalPath);
            else
                msg += " " + string.Format(CultureInfo.CurrentCulture, Constants.Notification_CreateNewProject, newrepo.LocalPath);

            // Team Explorer will show a notification that we want to override.
            // Put a small pause in here to make sure that it's appeared so we can clear it.
            await Task.Delay(100);

            ShowNotification(newrepo, msg);
        }

        void ShowNotification(ILocalRepositoryModel newrepo, string msg)
        {
            Guard.ArgumentNotNull(newrepo, nameof(newrepo));

            var teServices = ServiceProvider.TryGetService<ITeamExplorerServices>();

            teServices.ClearNotifications();
            teServices.ShowMessage(
                msg,
                new RelayCommand(o =>
                {
                    var str = o.ToString();
                    /* the prefix is the action to perform:
                     * u: launch browser with url
                     * c: launch create new project dialog
                     * o: launch open existing project dialog 
                    */
                    var prefix = str.Substring(0, 2);
                    if (prefix == "u:")
                        OpenInBrowser(ServiceProvider.TryGetService<IVisualStudioBrowser>(), new Uri(str.Substring(2)));
                    else if (prefix == "o:")
                    {
                        if (ErrorHandler.Succeeded(ServiceProvider.GetSolution().OpenSolutionViaDlg(str.Substring(2), 1)))
                            ServiceProvider.TryGetService<ITeamExplorer>()?.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null);
                    }
                    else if (prefix == "c:")
                    {
                        var vsGitServices = ServiceProvider.TryGetService<IVSGitServices>();
                        vsGitServices.SetDefaultProjectPath(newrepo.LocalPath);
                        if (ErrorHandler.Succeeded(ServiceProvider.GetSolution().CreateNewProjectViaDlg(null, null, 0)))
                            ServiceProvider.TryGetService<ITeamExplorer>()?.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null);
                    }
                })
            );
            log.Debug("Notification");
        }

        async Task RefreshRepositories()
        {
            // TODO: This is wasteful as we can be calling it multiple times for a single changed
            // signal, once from each section. Needs refactoring.
            await localRepositories.Refresh();
            RaisePropertyChanged("Repositories"); // trigger a re-check of the visibility of the listview based on item count
        }

        public async Task DoCreate()
        {
            ServiceProvider.GitServiceProvider = TEServiceProvider;
            var dialogService = ServiceProvider.GetService<IDialogService>();
            var result = await dialogService.ShowCreateRepositoryDialog(SectionConnection);

            if (result != null)
            {
                try
                {
                    ServiceProvider.GitServiceProvider = TEServiceProvider;
                    var createService = ServiceProvider.GetService<IRepositoryCreationService>();
                    var apiClientFactory = ServiceProvider.GetService<IApiClientFactory>();
                    var apiClient = await apiClientFactory.Create(SectionConnection.HostAddress);
                    var repo = await createService.CreateRepository(
                        result.NewRepository,
                        result.Account,
                        result.BaseRepositoryPath,
                        apiClient);
                    await HandleCreatedRepo(repo);
                    usageTracker.IncrementCounter(x => x.NumberOfGitHubConnectSectionClones).Forget();
                }
                catch (Exception e)
                {
                    var teServices = ServiceProvider.TryGetService<ITeamExplorerServices>();
                    var error = TranslateRepositoryCreateException(e, result.Account, result.NewRepository.Name);
                    teServices.ShowError(error.ErrorMessage + Environment.NewLine + error.ErrorCauseOrResolution);
                }
            }
        }

        public void SignOut()
        {
            connectionManager.LogOut(SectionConnection.HostAddress);
        }

        public void Login()
        {
            var dialogService = ServiceProvider.GetService<IDialogService>();
            dialogService.ShowLoginDialog();
        }

        public bool OpenRepository()
        {
            var old = Repositories.FirstOrDefault(x => x.Equals(Holder.ActiveRepo));
            if (!Equals(SelectedRepository, old))
            {
                var opened = vsServices.TryOpenRepository(SelectedRepository.LocalPath);
                if (!opened)
                {
                    // TryOpenRepository might fail because dir no longer exists. Let user find solution themselves.
                    opened = ErrorHandler.Succeeded(ServiceProvider.GetSolution().OpenSolutionViaDlg(SelectedRepository.LocalPath, 1));
                    if (!opened)
                    {
                        return false;
                    }
                }
            }

            // Navigate away when we're on the correct source control contexts.
            ServiceProvider.TryGetService<ITeamExplorer>()?.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null);
            return true;
        }

        bool disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    connectionManager.Connections.CollectionChanged -= RefreshConnections;
                    if (Repositories != null)
                        Repositories.CollectionChanged -= UpdateRepositoryList;
                    disposed = true;
                    packageSettings.Save();
                }
            }
            base.Dispose(disposing);
        }

        PublishRepositoryUserError TranslateRepositoryCreateException(Exception ex, IAccount account, string repositoryName)
        {
            Guard.ArgumentNotNull(ex, nameof(ex));

            if (ex is Octokit.RepositoryExistsException existsException)
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    App.Resources.RepositoryCreationFailedAlreadyExists,
                    account.Login, repositoryName);
                return new PublishRepositoryUserError(
                    message,
                    App.Resources.RepositoryCreationFailedAlreadyExistsMessage);
            }

            if (ex is Octokit.PrivateRepositoryQuotaExceededException quotaExceededException)
            {
                return new PublishRepositoryUserError(App.Resources.RepositoryCreationFailedQuota, quotaExceededException.Message);
            }

            return new PublishRepositoryUserError(ex.Message);
        }

        /// <summary>
        /// Creates a ReactiveCommand that works like a command created via
        /// <see cref="ReactiveCommand.CreateAsyncTask"/> but that does not hang when the async
        /// task shows a modal dialog.
        /// </summary>
        /// <param name="executeAsync">Method that creates the task to run.</param>
        /// <returns>A reactive command.</returns>
        /// <remarks>
        /// The <see cref="Clone"/> command needs to be disabled while a clone operation is in
        /// progress but also needs to display a modal dialog. For some reason using
        /// <see cref="ReactiveCommand.CreateAsyncTask"/> causes a weird UI hang in this situation
        /// where the UI runs but WhenAny no longer responds to property changed notifications.
        /// </remarks>
        static ReactiveCommand<object> CreateAsyncCommandHack(Func<Task> executeAsync)
        {
            Guard.ArgumentNotNull(executeAsync, nameof(executeAsync));

            var enabled = new BehaviorSubject<bool>(true);
            var command = ReactiveCommand.Create(enabled);
            command.Subscribe(async _ =>
            {
                enabled.OnNext(false);
                try { await executeAsync(); }
                finally { enabled.OnNext(true); }
            });
            return command;
        }

        class SectionStateTracker
        {
            enum SectionState
            {
                Idle,
                Busy,
                Refreshing
            }

            readonly Stateless.StateMachine<SectionState, string> machine;
            readonly ITeamExplorerSection section;

            public SectionStateTracker(ITeamExplorerSection section, Func<Task> onRefreshed)
            {
                this.section = section;
                machine = new Stateless.StateMachine<SectionState, string>(SectionState.Idle);
                machine.Configure(SectionState.Idle)
                    .PermitIf("IsBusy", SectionState.Busy, () => this.section.IsBusy)
                    .IgnoreIf("IsBusy", () => !this.section.IsBusy);
                machine.Configure(SectionState.Busy)
                    .Permit("Title", SectionState.Refreshing)
                    .PermitIf("IsBusy", SectionState.Idle, () => !this.section.IsBusy)
                    .IgnoreIf("IsBusy", () => this.section.IsBusy);
                machine.Configure(SectionState.Refreshing)
                    .Ignore("Title")
                    .PermitIf("IsBusy", SectionState.Idle, () => !this.section.IsBusy)
                    .IgnoreIf("IsBusy", () => this.section.IsBusy)
                    .OnExit(() => onRefreshed());

                section.PropertyChanged += TrackState;
            }
#if DEBUG
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
#endif
            void TrackState(object sender, PropertyChangedEventArgs e)
            {
                if (machine.PermittedTriggers.Contains(e.PropertyName))
                {
                    log.Debug("{PropertyName} title:{Title} busy:{IsBusy}",
                        e.PropertyName,
                        ((ITeamExplorerSection)sender).Title,
                        ((ITeamExplorerSection)sender).IsBusy);
                    machine.Fire(e.PropertyName);
                }
            }
        }
    }
}
