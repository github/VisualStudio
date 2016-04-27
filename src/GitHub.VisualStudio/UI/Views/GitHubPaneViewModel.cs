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
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio.UI.Views
{
    [ExportViewModel(ViewType = UIViewType.GitHubPane)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubPaneViewModel : TeamExplorerItemBase, IGitHubPaneViewModel
    {
        CompositeDisposable disposables = new CompositeDisposable();
        IUIController uiController;
        WindowController windowController;

        readonly IRepositoryHosts hosts;
        readonly SynchronizationContext syncContext;
        readonly IConnectionManager connectionManager;

        readonly List<ViewWithData> navStack = new List<ViewWithData>();
        int currentNavItem = -1;
        bool navigatingViaArrows;
        OleMenuCommand back, forward, refresh;

        [ImportingConstructor]
        public GitHubPaneViewModel(ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder,
            IConnectionManager cm, IRepositoryHosts hosts)
            : base(apiFactory, holder)
        {
            this.connectionManager = cm;
            this.hosts = hosts;
            syncContext = SynchronizationContext.Current;
            CancelCommand = ReactiveCommand.Create();
            Title = "GitHub";
        }

        public override void Initialize(IServiceProvider serviceProvider)
        {
            base.Initialize(serviceProvider);

            ServiceProvider.AddCommandHandler(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.pullRequestCommand,
                (s, e) => Reload(new ViewWithData { Flow = UIControllerFlow.PullRequests, ViewType = UIViewType.PRList }));

            back = ServiceProvider.AddCommandHandler(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.backCommand,
                () => !disabled && currentNavItem > 0,
                () => {
                    DisableButtons();
                    Reload(navStack[--currentNavItem], true);
                },
                true);

            forward = ServiceProvider.AddCommandHandler(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.forwardCommand,
                () => !disabled && currentNavItem < navStack.Count - 1,
                () => {
                    DisableButtons();
                    Reload(navStack[++currentNavItem], true);
                },
                true);

            refresh = ServiceProvider.AddCommandHandler(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.refreshCommand,
                () => !disabled && navStack.Count > 0,
                () => {
                    DisableButtons();
                    Reload();
                },
                true);
        }

        public void Initialize([AllowNull] ViewWithData data)
        {
            Title = "GitHub";
            Reload(data);
        }

        protected async override void RepoChanged(bool changed)
        {
            base.RepoChanged(changed);

            if (!changed)
                return;

            IsGitHubRepo = await IsAGitHubRepo();
            if (!IsGitHubRepo)
                return;

            Reload();
        }

        async void Reload([AllowNull] ViewWithData data = null, bool navigating = false)
        {
            navigatingViaArrows = navigating;

            if (!IsGitHubRepo)
            {
                if (uiController != null)
                {
                    Stop();
                    //var factory = ServiceProvider.GetExportedValue<IUIFactory>();
                    //var c = factory.CreateViewAndViewModel(UIViewType.LoggedOut);
                    //Control = c.View;
                }
                return;
            }

            var connection = await connectionManager.LookupConnection(ActiveRepo);
            IsLoggedIn = await connection.IsLoggedIn(hosts);

            if (IsLoggedIn)
            {
                if (uiController == null || (data != null && data.Flow != uiController.CurrentFlow))
                    StartFlow(data?.Flow ?? UIControllerFlow.PullRequests, connection, data);
                else if (data != null || currentNavItem >= 0)
                    uiController.Jump(data ?? navStack[currentNavItem]);
            }
            else
            {
                var factory = ServiceProvider.GetExportedValue<IUIFactory>();
                var c = factory.CreateViewAndViewModel(UIViewType.LoggedOut);
                Control = c.View;
            }
        }

        void StartFlow(UIControllerFlow controllerFlow, [AllowNull]IConnection conn, ViewWithData data = null)
        {
            if (uiController != null)
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
            windowController?.Close();
            uiController.Stop();
            disposables.Clear();
            uiController = null;
            currentNavItem = -1;
            navStack.Clear();
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

        bool isGitHubRepo;
        public bool IsGitHubRepo
        {
            get { return isGitHubRepo; }
            set { isGitHubRepo = value; this.RaisePropertyChange(); }
        }


        public ReactiveCommand<object> CancelCommand { get; private set; }
        public ICommand Cancel => CancelCommand;

        public bool IsShowing => true;

        bool disposed = false;
        private bool disabled;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    disposables.Dispose();
                    disposed = true;
                }
            }
            base.Dispose(disposing);
        }
    }
}