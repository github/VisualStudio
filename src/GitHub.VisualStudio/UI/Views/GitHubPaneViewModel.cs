using System;
using System.ComponentModel.Composition;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using GitHub.Api;
using GitHub.App.Factories;
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
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.VisualStudio.UI;
using System.Windows.Threading;
using GitHub.VisualStudio.UI.Controls;

namespace GitHub.VisualStudio.UI.Views
{
    [ExportViewModel(ViewType = UIViewType.GitHubPane)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubPaneViewModel : TeamExplorerItemBase, IGitHubPaneViewModel
    {
        const UIControllerFlow DefaultControllerFlow = UIControllerFlow.PullRequests;

        bool initialized;
        readonly CompositeDisposable disposables = new CompositeDisposable();
        IUIController uiController;
        WindowController windowController;

        readonly IRepositoryHosts hosts;
        readonly SynchronizationContext syncContext;
        readonly IConnectionManager connectionManager;

        readonly List<ViewWithData> navStack = new List<ViewWithData>();
        int currentNavItem = -1;
        bool navigatingViaArrows;
        bool disabled;
        Microsoft.VisualStudio.Shell.OleMenuCommand back, forward, refresh;
        int latestReloadCallId;

        [ImportingConstructor]
        public GitHubPaneViewModel(ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder,
            IConnectionManager cm, IRepositoryHosts hosts, INotificationDispatcher notifications)
            : base(apiFactory, holder)
        {
            this.connectionManager = cm;
            this.hosts = hosts;
            syncContext = SynchronizationContext.Current;
            CancelCommand = ReactiveCommand.Create();
            Title = "GitHub";
            Message = String.Empty;
        }

        public override void Initialize(IServiceProvider serviceProvider)
        {
            serviceProvider.AddCommandHandler(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.pullRequestCommand,
                (s, e) => Reload(new ViewWithData(UIControllerFlow.PullRequests) { ViewType = UIViewType.PRList }).Forget());

            back = serviceProvider.AddCommandHandler(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.backCommand,
                () => !disabled && currentNavItem > 0,
                () => {
                    DisableButtons();
                    Reload(navStack[--currentNavItem], true).Forget();
                },
                true);

            forward = serviceProvider.AddCommandHandler(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.forwardCommand,
                () => !disabled && currentNavItem < navStack.Count - 1,
                () => {
                    DisableButtons();
                    Reload(navStack[++currentNavItem], true).Forget();
                },
                true);

            refresh = serviceProvider.AddCommandHandler(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.refreshCommand,
                () => !disabled && navStack.Count > 0,
                () => {
                    DisableButtons();
                    Reload().Forget();
                },
                true);

            initialized = true;

            base.Initialize(serviceProvider);

            hosts.WhenAnyValue(x => x.IsLoggedInToAnyHost).Subscribe(_ => Reload().Forget());
        }

        public void Initialize([AllowNull] ViewWithData data)
        {
            Title = "GitHub";
            Reload(data).Forget();
        }

        protected override void RepoChanged(bool changed)
        {
            base.RepoChanged(changed);

            if (!initialized)
                return;

            if (!changed)
                return;

            Stop();
            RepositoryOrigin = RepositoryOrigin.Unknown;
            Reload().Forget();
        }

        /// <summary>
        /// This method is reentrant, so all await calls need to be done before
        /// any actions are performed on the data. More recent calls to this method
        /// will cause previous calls pending on await calls to exit early.
        /// </summary>
        /// <returns></returns>
        async Task Reload([AllowNull] ViewWithData data = null, bool navigating = false)
        {
            if (!initialized)
                return;

            latestReloadCallId++;
            var reloadCallId = latestReloadCallId;

            navigatingViaArrows = navigating;

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

            if (RepositoryOrigin == UI.RepositoryOrigin.NonGitRepository)
            {
                LoadView(UIViewType.NotAGitRepository);
            }
            else if (RepositoryOrigin == UI.RepositoryOrigin.Other)
            {
                LoadView(UIViewType.NotAGitHubRepository);
            }
            else if (!IsLoggedIn)
            {
                LoadView(UIViewType.LoggedOut);
            }
            else
            {
                LoadView(data?.ActiveFlow ?? DefaultControllerFlow, connection, data);
            }
        }

        void LoadView(UIControllerFlow flow, IConnection connection = null, ViewWithData data = null, UIViewType type = UIViewType.None)
        {
            // if we're loading a single view or a different flow, we need to stop the current controller
            var restart = flow == UIControllerFlow.None || uiController?.SelectedFlow != flow;

            if (restart)
                Stop();

            // if there's no selected flow, then just load a view directly
            if (flow == UIControllerFlow.None)
            {
                var factory = ServiceProvider.GetExportedValue<IUIFactory>();
                var c = factory.CreateViewAndViewModel(type);
                c.View.DataContext = c.ViewModel;
                Control = c.View;
            }
            // it's a new flow!
            else if (restart)
            {
                StartFlow(flow, connection, data);
            }
            // navigate to a requested view within the currently running uiController
            else
            {
                uiController.Jump(data ?? navStack[currentNavItem]);
            }
        }

        void LoadView(UIViewType type)
        {
            LoadView(UIControllerFlow.None, type: type);
        }

        void StartFlow(UIControllerFlow controllerFlow, [AllowNull]IConnection conn, ViewWithData data = null)
        {
            Stop();

            if (conn == null)
                return;

            switch (controllerFlow)
            {
                case UIControllerFlow.PullRequests:
                    Title = Resources.PullRequestsNavigationItemText;
                    break;
                default:
                    Title = "GitHub";
                    break;
            }
            
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            var factory = uiProvider.GetService<IExportFactoryProvider>();
            var uiflow = factory.UIControllerFactory.CreateExport();
            disposables.Add(uiflow);
            uiController = uiflow.Value;
            var creation = uiController.SelectFlow(controllerFlow).Publish().RefCount();

            // if the flow is authentication, we need to show the login dialog. and we can't
            // block the main thread on the subscriber, it'll block other handlers, so we're doing
            // this on a separate thread and posting the dialog to the main thread
            creation
                .Where(c => uiController.CurrentFlow == UIControllerFlow.Authentication)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Subscribe(c =>
                {
                    // nothing to do, we already have a dialog
                    if (windowController != null)
                        return;
                    syncContext.Post(_ =>
                    {
                        windowController = new WindowController(creation,
                            __ => uiController.CurrentFlow == UIControllerFlow.Authentication,
                            ___ => uiController.CurrentFlow != UIControllerFlow.Authentication);
                        windowController.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        windowController.Load(c.View);
                        windowController.ShowModal();
                        windowController = null;
                    }, null);
                });

            creation
                .Where(c => uiController.CurrentFlow != UIControllerFlow.Authentication)
                .Subscribe(c =>
                {
                    if (!navigatingViaArrows)
                    {
                        if (c.Direction == LoadDirection.Forward)
                            GoForward(c.Data);
                        else if (c.Direction == LoadDirection.Back)
                            GoBack();
                    }
                    UpdateToolbar();

                    Control = c.View;
                });

            if (data != null)
                uiController.Jump(data);
            uiController.Start(conn);
        }

        void GoForward(ViewWithData data)
        {
            currentNavItem++;
            if (currentNavItem < navStack.Count - 1)
                navStack.RemoveRange(currentNavItem, navStack.Count - 1 - currentNavItem);
            navStack.Add(data);
        }

        void GoBack()
        {
            navStack.RemoveRange(currentNavItem, navStack.Count - 1 - currentNavItem);
            currentNavItem--;
        }

        void UpdateToolbar()
        {
            back.Enabled = currentNavItem > 0;
            forward.Enabled = currentNavItem < navStack.Count - 1;
            refresh.Enabled = navStack.Count > 0;
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
            if (uiController == null)
                return;

            DisableButtons();
            windowController?.Close();
            uiController.Stop();
            disposables.Clear();
            uiController = null;
            currentNavItem = -1;
            navStack.Clear();
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
            [return: AllowNull] get { return control; }
            set { control = value; this.RaisePropertyChange(); }
        }

        bool isLoggedIn;
        public bool IsLoggedIn
        {
            get { return isLoggedIn; }
            set { isLoggedIn = value;  this.RaisePropertyChange(); }
        }

        RepositoryOrigin repositoryOrigin;
        public RepositoryOrigin RepositoryOrigin
        {
            get { return repositoryOrigin; }
            private set { repositoryOrigin = value; }
        }

        string message;
        [AllowNull]
        public string Message
        {
            [return:AllowNull] get { return message; }
            set { message = value; this.RaisePropertyChange(); }
        }

        MessageType messageType;
        [AllowNull]
        public MessageType MessageType
        {
            [return: AllowNull]
            get { return messageType; }
            set { messageType = value; this.RaisePropertyChange(); }
        }

        public bool? IsGitHubRepo
        {
            get
            {
                return repositoryOrigin == RepositoryOrigin.Unknown ?
                    (bool?)null :
                    repositoryOrigin == UI.RepositoryOrigin.DotCom ||
                    repositoryOrigin == UI.RepositoryOrigin.Enterprise;
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

        public void Reset()
        {
        }
    }
}
