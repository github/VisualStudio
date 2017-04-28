using GitHub.Api;
using GitHub.Controllers;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using NullGuard;
using ReactiveUI;
using System;
using System.ComponentModel.Composition;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GitHub.Info;

namespace GitHub.VisualStudio.UI.Views
{
    [ExportViewModel(ViewType = UIViewType.GitHubPane)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [NullGuard(ValidationFlags.None)]
    public class GitHubPaneViewModel : TeamExplorerItemBase, IGitHubPaneViewModel
    {
        const UIControllerFlow DefaultControllerFlow = UIControllerFlow.PullRequestList;

        bool initialized;
        readonly CompositeDisposable disposables = new CompositeDisposable();

        readonly IRepositoryHosts hosts;
        readonly IConnectionManager connectionManager;
        readonly IUIProvider uiProvider;
        readonly IVisualStudioBrowser browser;
        readonly IUsageTracker usageTracker;
        NavigationController navController;

        bool disabled;
        Microsoft.VisualStudio.Shell.OleMenuCommand back, forward, refresh;
        int latestReloadCallId;

        [ImportingConstructor]
        public GitHubPaneViewModel(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder,
            IConnectionManager cm, IRepositoryHosts hosts, IUIProvider uiProvider, IVisualStudioBrowser vsBrowser,
            IUsageTracker usageTracker)
            : base(serviceProvider, apiFactory, holder)
        {
            this.connectionManager = cm;
            this.hosts = hosts;
            this.uiProvider = uiProvider;
            this.usageTracker = usageTracker;

            CancelCommand = ReactiveCommand.Create();
            Title = "GitHub";
            Message = String.Empty;
            browser = vsBrowser;

            this.WhenAnyValue(x => x.Control.DataContext)
                .OfType<IPanePageViewModel>()
                .Select(x => x.WhenAnyValue(y => y.Title))
                .Switch()
                .Subscribe(x => Title = x ?? "GitHub");
        }

        public override void Initialize(IServiceProvider serviceProvider)
        {
            serviceProvider.AddCommandHandler(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.pullRequestCommand,
                (s, e) => Load(new ViewWithData(UIControllerFlow.PullRequestList)).Forget());

            back = serviceProvider.AddCommandHandler(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.backCommand,
                () => !disabled && (navController?.HasBack ?? false),
                () =>
                {
                    DisableButtons();
                    navController.Back();
                },
                true);

            forward = serviceProvider.AddCommandHandler(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.forwardCommand,
                () => !disabled && (navController?.HasForward ?? false),
                () =>
                {
                    DisableButtons();
                    navController.Forward();
                },
                true);

            refresh = serviceProvider.AddCommandHandler(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.refreshCommand,
                () => !disabled,
                () =>
                {
                    DisableButtons();
                    Refresh();
                },
                true);

            serviceProvider.AddCommandHandler(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.githubCommand,
                () => !disabled && (RepositoryOrigin == RepositoryOrigin.DotCom || RepositoryOrigin == RepositoryOrigin.Enterprise),
                () =>
                {
                    switch (navController?.Current.CurrentFlow)
                    {
                        case UIControllerFlow.PullRequestDetail:
                            var prDetailViewModel = control.DataContext as IPullRequestDetailViewModel;
                            if (prDetailViewModel != null)
                            {
                                browser.OpenUrl(ActiveRepoUri.ToRepositoryUrl().Append("pull/" + prDetailViewModel.Model.Number));
                            }
                            else
                            {
                                goto default;
                            }
                            break;

                        case UIControllerFlow.PullRequestList:
                        case UIControllerFlow.PullRequestCreation:
                            browser.OpenUrl(ActiveRepoUri.ToRepositoryUrl().Append("pulls/"));
                            break;

                        case UIControllerFlow.Home:
                        default:
                            browser.OpenUrl(ActiveRepoUri.ToRepositoryUrl());
                            break;
                    }
                },
                true);

           serviceProvider.AddCommandHandler(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.helpCommand,
                () => true,
                () =>
                {
                    browser.OpenUrl(new Uri(GitHubUrls.Documentation));
                    usageTracker.IncrementGitHubPaneHelpClicks().Forget();
                },
                true);

            initialized = true;

            base.Initialize(serviceProvider);

            hosts.WhenAnyValue(x => x.IsLoggedInToAnyHost).Subscribe(_ => LoadDefault());
        }

        public void Initialize(ViewWithData data = null)
        {
            if (!initialized)
                return;

            Title = "GitHub";
            Load(data).Forget();
        }

        void SetupNavigation()
        {
            navController = new NavigationController(uiProvider);
            navController
                .WhenAnyValue(x => x.HasBack, y => y.HasForward, z => z.Current)
                .Where(_ => !navController.IsBusy)
                .Subscribe(_ => UpdateToolbar());

            navController
                .WhenAnyValue(x => x.IsBusy)
                .Subscribe(v =>
                {
                    if (v)
                        DisableButtons();
                    else
                        UpdateToolbar();
                });
        }

        protected override void RepoChanged(bool changed)
        {
            base.RepoChanged(changed);

            if (!initialized)
                return;

            if (changed)
            {
                Stop();
                RepositoryOrigin = RepositoryOrigin.Unknown;
            }

            Refresh();
        }

        void LoadDefault()
        {
            Load(new ViewWithData(DefaultControllerFlow)).Forget();
        }

        void Refresh()
        {
            Load(null).Forget();
        }

        /// <summary>
        /// This method is reentrant, so all await calls need to be done before
        /// any actions are performed on the data. More recent calls to this method
        /// will cause previous calls pending on await calls to exit early.
        /// </summary>
        /// <returns></returns>
        async Task Load(ViewWithData data)
        {
            if (!initialized)
                return;

            latestReloadCallId++;
            var reloadCallId = latestReloadCallId;

            if (RepositoryOrigin == RepositoryOrigin.Unknown)
            {
                var origin = await GetRepositoryOrigin();
                if (reloadCallId != latestReloadCallId)
                    return;

                RepositoryOrigin = origin;
            }

            var connection = await connectionManager.LookupConnection(ActiveRepo);
            if (reloadCallId != latestReloadCallId)
                return;

            if (connection == null)
                IsLoggedIn = false;
            else
            {
                var isLoggedIn = await connection.IsLoggedIn(hosts);
                if (reloadCallId != latestReloadCallId)
                    return;

                IsLoggedIn = isLoggedIn;
            }

            Load(connection, data);
        }

        void Load(IConnection connection, ViewWithData data)
        {
            if (RepositoryOrigin == UI.RepositoryOrigin.NonGitRepository)
            {
                LoadSingleView(UIViewType.NotAGitRepository, data);
            }
            else if (RepositoryOrigin == UI.RepositoryOrigin.Other)
            {
                LoadSingleView(UIViewType.NotAGitHubRepository, data);
            }
            else if (!IsLoggedIn)
            {
                LoadSingleView(UIViewType.LoggedOut, data);
            }
            else
            {
                var flow = DefaultControllerFlow;
                if (navController != null)
                {
                    flow = navController.Current.SelectedFlow;
                }
                else
                {
                    SetupNavigation();
                }

                if (data == null)
                    data = new ViewWithData(flow);

                navController.LoadView(connection, data, view =>
                {
                    Control = view;
                    UpdateToolbar();
                });
            }
        }

        void LoadSingleView(UIViewType type, ViewWithData data)
        {
            Stop();
            Control = uiProvider.GetView(type, data);
        }

        void UpdateToolbar()
        {
            back.Enabled = navController?.HasBack ?? false;
            forward.Enabled = navController?.HasForward ?? false;
            refresh.Enabled = navController?.Current != null;
            disabled = false;
        }

        void DisableButtons()
        {
            disabled = true;
            back.Enabled = false;
            forward.Enabled = false;
            refresh.Enabled = false;
        }

        void Stop()
        {
            DisableButtons();
            navController = null;
            disposables.Clear();
            UpdateToolbar();
        }

        string title;
        [AllowNull]
        public string Title
        {
            get { return title; }
            set { title = value; this.RaisePropertyChange(); }
        }

        IView control;
        public IView Control
        {
            get { return control; }
            set { control = value; this.RaisePropertyChange(); }
        }

        bool isLoggedIn;
        public bool IsLoggedIn
        {
            get { return isLoggedIn; }
            set { isLoggedIn = value; this.RaisePropertyChange(); }
        }

        public RepositoryOrigin RepositoryOrigin { get; private set; }

        string message;
        [AllowNull]
        public string Message
        {
            get { return message; }
            set { message = value; this.RaisePropertyChange(); }
        }

        MessageType messageType;
        [AllowNull]
        public MessageType MessageType
        {
            get { return messageType; }
            set { messageType = value; this.RaisePropertyChange(); }
        }

        public bool? IsGitHubRepo
        {
            get
            {
                return RepositoryOrigin == RepositoryOrigin.Unknown ?
                    (bool?)null :
                    RepositoryOrigin == UI.RepositoryOrigin.DotCom ||
                    RepositoryOrigin == UI.RepositoryOrigin.Enterprise;
            }
        }

        public ReactiveCommand<object> CancelCommand { get; private set; }
        public ICommand Cancel => CancelCommand;

        public bool IsShowing => true;

        bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    disposed = true;
                    DisableButtons();
                    disposables.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
