#pragma warning disable 169
using System;
using System.ComponentModel.Composition;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GitHub.Api;
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
using GitHub.App.Factories;

namespace GitHub.VisualStudio.UI.Views
{
    [ExportViewModel(ViewType = UIViewType.GitHubPane)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubPaneViewModel : TeamExplorerItemBase, IGitHubPaneViewModel
    {
        CompositeDisposable disposables = new CompositeDisposable();
        IUIController uiController;
        WindowController windowController;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        bool loggedIn;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        readonly IRepositoryHosts hosts;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        readonly SynchronizationContext syncContext;
        readonly IConnectionManager connectionManager;

        [ImportingConstructor]
        public GitHubPaneViewModel(ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder,
            IConnectionManager cm, IRepositoryHosts hosts)
            : base(apiFactory, holder)
        {
            this.connectionManager = cm;
            this.hosts = hosts;
            syncContext = SynchronizationContext.Current;
            CancelCommand = ReactiveCommand.Create();
        }

        public override void Initialize(IServiceProvider serviceProvider)
        {
            base.Initialize(serviceProvider);

            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.pullRequestCommand,
                async (s, e) => {
                    var connection = await connectionManager.LookupConnection(ActiveRepo);
                    StartFlow(UIControllerFlow.PullRequests, connection);
                });

            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.backCommand,
                (s, e) => { });

            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.forwardCommand,
                (s, e) => { });

            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.refreshCommand,
                (s, e) => { });
        }

        public async void Initialize([AllowNull] ViewWithData data)
        {
            if (!isGitHubRepo || !IsLoggedIn)
                return;

            if (uiController == null)
            {
                var connection = await connectionManager.LookupConnection(ActiveRepo);
                StartFlow(data.Flow, connection, data);
            }
            else
                uiController.Jump(data);
        }

        protected async override void RepoChanged(bool changed)
        {
            base.RepoChanged(changed);

            if (!changed)
                return;

            IsGitHubRepo = await IsAGitHubRepo();
            if (!IsGitHubRepo)
                return;
                
            var connection = await connectionManager.LookupConnection(ActiveRepo);
            IsLoggedIn = await connection.IsLoggedIn(hosts);

            if (IsLoggedIn)
                StartFlow(UIControllerFlow.PullRequests, connection);
            else
            {
                //var factory = ServiceProvider.GetExportedValue<IUIFactory>();
                //var c = factory.CreateViewAndViewModel(UIViewType.LoggedOut);
                //Control = c.View;
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
                        windowController.Load(c);
                        windowController.ShowModal();
                        windowController = null;
                    }, null);
                });

            creation
                .Where(c => uiController.CurrentFlow != UIControllerFlow.Authentication)
                .Subscribe(c =>
                {
                    Control = c;
                });

            if (data != null)
                uiController.Jump(data);
            uiController.Start(conn);
        }

        void Stop()
        {
            windowController?.Close();
            uiController.Stop();
            disposables.Clear();
            uiController = null;
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