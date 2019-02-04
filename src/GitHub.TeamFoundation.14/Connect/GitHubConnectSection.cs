using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.Settings;
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
        readonly ISimpleApiClientFactory apiFactory;
        readonly ITeamExplorerServiceHolder holder;
        readonly IPackageSettings packageSettings;
        readonly ITeamExplorerServices teamExplorerServices;
        readonly int sectionIndex;
        readonly ILocalRepositories localRepositories;
        readonly IUsageTracker usageTracker;

        ITeamExplorerSection invitationSection;
        string errorMessage;
        bool isCloning;
        bool isCreating;
        GitHubConnectSectionState settings;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        SectionStateTracker sectionTracker;

        protected GitHubConnectContent View
        {
            get { return SectionContent as GitHubConnectContent; }
            private set { SectionContent = value; }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
            private set { errorMessage = value; this.RaisePropertyChange(); }
        }

        public IConnection SectionConnection { get; set; }

        bool isLoggingIn;
        public bool IsLoggingIn
        {
            get { return isLoggingIn; }
            private set { isLoggingIn = value; this.RaisePropertyChange(); }
        }

        bool showLogin;
        public bool ShowLogin
        {
            get { return showLogin; }
            private set { showLogin = value; this.RaisePropertyChange(); }
        }

        bool showLogout;
        public bool ShowLogout
        {
            get { return showLogout; }
            private set { showLogout = value; this.RaisePropertyChange(); }
        }

        bool showRetry;
        public bool ShowRetry
        {
            get { return showRetry; }
            private set { showRetry = value; this.RaisePropertyChange(); }
        }

        IReactiveDerivedList<LocalRepositoryModel> repositories;
        public IReactiveDerivedList<LocalRepositoryModel> Repositories
        {
            get { return repositories; }
            private set { repositories = value; this.RaisePropertyChange(); }
        }

        LocalRepositoryModel selectedRepository;
        public LocalRepositoryModel SelectedRepository
        {
            get { return selectedRepository; }
            set { selectedRepository = value; this.RaisePropertyChange(); }
        }

        public ICommand Clone { get; }

        public GitHubConnectSection(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory,
            ITeamExplorerServiceHolder holder,
            IConnectionManager manager,
            IPackageSettings packageSettings,
            ITeamExplorerServices teamExplorerServices,
            ILocalRepositories localRepositories,
            IUsageTracker usageTracker,
            int index)
            : base(serviceProvider, apiFactory, holder, manager)
        {
            Guard.ArgumentNotNull(apiFactory, nameof(apiFactory));
            Guard.ArgumentNotNull(holder, nameof(holder));
            Guard.ArgumentNotNull(manager, nameof(manager));
            Guard.ArgumentNotNull(packageSettings, nameof(packageSettings));
            Guard.ArgumentNotNull(teamExplorerServices, nameof(teamExplorerServices));
            Guard.ArgumentNotNull(localRepositories, nameof(localRepositories));
            Guard.ArgumentNotNull(usageTracker, nameof(usageTracker));

            Title = "GitHub";
            IsEnabled = true;
            IsVisible = false;
            sectionIndex = index;

            this.apiFactory = apiFactory;
            this.holder = holder;
            this.packageSettings = packageSettings;
            this.teamExplorerServices = teamExplorerServices;
            this.localRepositories = localRepositories;
            this.usageTracker = usageTracker;

            Clone = ReactiveCommand.CreateFromTask(DoClone);

            ConnectionManager.Connections.CollectionChanged += RefreshConnections;
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
                    await cloneService.CloneOrOpenRepository(result);

                    usageTracker.IncrementCounter(x => x.NumberOfGitHubConnectSectionClones).Forget();
                }
                catch (Exception e)
                {
                    var teServices = ServiceProvider.TryGetService<ITeamExplorerServices>();
                    teServices.ShowError(e.GetUserFriendlyErrorMessage(ErrorType.CloneOrOpenFailed, result.Url.RepositoryName));
                }
            }
        }

        void RefreshConnections(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (ConnectionManager.Connections.Count > sectionIndex)
                        Refresh(ConnectionManager.Connections[sectionIndex]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Refresh(ConnectionManager.Connections.Count <= sectionIndex
                        ? null
                        : ConnectionManager.Connections[sectionIndex]);
                    break;
            }
        }

        protected void Refresh(IConnection connection)
        {
            InitializeInvitationSection();

            ErrorMessage = connection?.ConnectionError?.GetUserFriendlyErrorMessage(ErrorType.LoginFailed);
            IsLoggingIn = connection?.IsLoggingIn ?? false;
            IsVisible = connection != null || (invitationSection?.IsVisible == false);

            if (connection == null || !connection.IsLoggedIn)
            {
                if (Repositories != null)
                    Repositories.CollectionChanged -= UpdateRepositoryList;
                Repositories = null;
                settings = null;

                if (connection?.ConnectionError != null)
                {
                    ShowLogin = false;
                    ShowLogout = true;
                    ShowRetry = !(connection.ConnectionError is Octokit.AuthorizationException);
                }
                else
                {
                    ShowLogin = true;
                    ShowLogout = false;
                    ShowRetry = false;
                }
            }
            else if (connection != SectionConnection || Repositories == null)
            {
                Repositories?.Dispose();
                Repositories = localRepositories.GetRepositoriesForAddress(connection.HostAddress);
                Repositories.CollectionChanged += UpdateRepositoryList;
                settings = packageSettings.UIState.GetOrCreateConnectSection(Title);
                ShowLogin = false;
                ShowLogout = true;
                Title = connection.HostAddress.Title;
            }

            if (connection != null && TEServiceProvider != null)
            {
                RefreshRepositories().Forget();
            }

            if (SectionConnection != connection)
            {
                if (SectionConnection != null)
                {
                    SectionConnection.PropertyChanged -= ConnectionPropertyChanged;
                }

                SectionConnection = connection;

                if (SectionConnection != null)
                {
                    SectionConnection.PropertyChanged += ConnectionPropertyChanged;
                }
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

        void InitializeInvitationSection()
        {
            // We're only interested in the invitation section if sectionIndex == 0. Don't want to show
            // two "Log In" options.
            if (sectionIndex == 0 && invitationSection == null)
            {
                invitationSection = GetSection(TeamExplorerInvitationBase.TeamExplorerInvitationSectionGuid);

                if (invitationSection != null)
                {
                    invitationSection.PropertyChanged += InvitationSectionPropertyChanged;
                }
            }
        }

        void UpdateConnection()
        {
            Refresh(ConnectionManager.Connections.Count > sectionIndex
                ? ConnectionManager.Connections[sectionIndex]
                : SectionConnection);
        }

        void OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsVisible) && IsVisible && View == null)
                View = new GitHubConnectContent { DataContext = this };
            else if (e.PropertyName == nameof(IsExpanded) && settings != null)
                settings.IsExpanded = IsExpanded;
        }

        void InvitationSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ITeamExplorerSection.IsVisible))
            {
                Refresh(SectionConnection);
            }
        }

        private void ConnectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IConnection.IsLoggedIn) ||
                e.PropertyName == nameof(IConnection.IsLoggingIn))
            {
                Refresh(SectionConnection);
            }
        }

        async void UpdateRepositoryList(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // if we're cloning or creating, only one repo will be added to the list
                // so we can handle just one new entry separately
                if (isCloning || isCreating)
                {
                    var newrepo = e.NewItems.Cast<LocalRepositoryModel>().First();

                    SelectedRepository = newrepo;
                    if (isCreating)
                        HandleCreatedRepo(newrepo);
                    else
                        HandleClonedRepo(newrepo);

                    isCreating = isCloning = false;

                    try
                    {
                        // TODO: Cache the icon state.
                        var api = await apiFactory.Create(newrepo.CloneUrl);
                        var repo = await api.GetRepository();
                        newrepo.SetIcon(repo.Private, repo.Fork);
                    }
                    catch (Exception ex)
                    {
                        // GetRepository() may throw if the user doesn't have permissions to access the repo
                        // (because the repo no longer exists, or because the user has logged in on a different
                        // profile, or their permissions have changed remotely)
                        log.Error(ex, "Error updating repository list");
                    }
                }
                // looks like it's just a refresh with new stuff on the list, update the icons
                else
                {
                    e.NewItems
                        .Cast<LocalRepositoryModel>()
                        .ForEach(async r =>
                    {
                        if (Equals(holder.TeamExplorerContext.ActiveRepository, r))
                            SelectedRepository = r;

                        try
                        {
                            // TODO: Cache the icon state.
                            var api = await apiFactory.Create(r.CloneUrl);
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
        }

        void HandleCreatedRepo(LocalRepositoryModel newrepo)
        {
            Guard.ArgumentNotNull(newrepo, nameof(newrepo));

            var msg = string.Format(CultureInfo.CurrentCulture, Constants.Notification_RepoCreated, newrepo.Name, newrepo.CloneUrl);
            msg += " " + string.Format(CultureInfo.CurrentCulture, Constants.Notification_CreateNewProject, newrepo.LocalPath);
            ShowNotification(newrepo, msg);
        }

        void HandleClonedRepo(LocalRepositoryModel newrepo)
        {
            Guard.ArgumentNotNull(newrepo, nameof(newrepo));

            var msg = string.Format(CultureInfo.CurrentCulture, Constants.Notification_RepoCloned, newrepo.Name, newrepo.CloneUrl);
            if (newrepo.HasCommits() && newrepo.MightContainSolution())
                msg += " " + string.Format(CultureInfo.CurrentCulture, Constants.Notification_OpenProject, newrepo.LocalPath);
            else
                msg += " " + string.Format(CultureInfo.CurrentCulture, Constants.Notification_CreateNewProject, newrepo.LocalPath);
            ShowNotification(newrepo, msg);
        }

        void ShowNotification(LocalRepositoryModel newrepo, string msg)
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
            RaisePropertyChanged(nameof(Repositories)); // trigger a re-check of the visibility of the listview based on item count
        }

        public void DoCreate()
        {
            ServiceProvider.GitServiceProvider = TEServiceProvider;
            var dialogService = ServiceProvider.GetService<IDialogService>();
            dialogService.ShowCreateRepositoryDialog(SectionConnection);
        }

        public void SignOut()
        {
            ConnectionManager.LogOut(SectionConnection.HostAddress);
        }

        public void Login()
        {
            var dialogService = ServiceProvider.GetService<IDialogService>();
            dialogService.ShowLoginDialog();
        }

        public void Retry()
        {
            ConnectionManager.Retry(SectionConnection);
        }

        public bool OpenRepository()
        {
            var old = Repositories.FirstOrDefault(x => x.Equals(holder.TeamExplorerContext.ActiveRepository));
            if (!Equals(SelectedRepository, old))
            {
                try
                {
                    var repositoryPath = SelectedRepository.LocalPath;
                    if (Directory.Exists(repositoryPath))
                    {
                        teamExplorerServices.OpenRepository(SelectedRepository.LocalPath);
                    }
                    else
                    {
                        // If directory no longer exists, let user find solution themselves
                        var opened = ErrorHandler.Succeeded(ServiceProvider.GetSolution().OpenSolutionViaDlg(SelectedRepository.LocalPath, 1));
                        if (!opened)
                        {
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error(e, nameof(OpenRepository));
                    return false;
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
                    ConnectionManager.Connections.CollectionChanged -= RefreshConnections;
                    if (Repositories != null)
                        Repositories.CollectionChanged -= UpdateRepositoryList;
                    disposed = true;
                    packageSettings.Save();
                }
            }
            base.Dispose(disposing);
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
        ////static ReactiveCommand<Unit,Unit> CreateAsyncCommandHack(Func<Task> executeAsync)
        ////{
        ////    Guard.ArgumentNotNull(executeAsync, nameof(executeAsync));

        ////    var enabled = new BehaviorSubject<bool>(true);
        ////    var command = ReactiveCommand.Create(enabled);
        ////    command.Subscribe(async _ =>
        ////    {
        ////        enabled.OnNext(false);
        ////        try { await executeAsync(); }
        ////        finally { enabled.OnNext(true); }
        ////    });
        ////    return command;
        ////}

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
