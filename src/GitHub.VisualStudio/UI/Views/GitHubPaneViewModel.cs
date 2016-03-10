#pragma warning disable 169
using System;
using System.ComponentModel.Composition;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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

namespace GitHub.VisualStudio.UI.Views
{
    [ExportViewModel(ViewType = UIViewType.GitHubPane)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubPaneViewModel : TeamExplorerSectionBase, IGitHubPaneViewModel
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

        [ImportingConstructor]
        public GitHubPaneViewModel(ITeamExplorerServiceHolder holder, IConnectionManager cm,
            IRepositoryHosts hosts)
            : base(holder, cm)
        {
            this.hosts = hosts;
            syncContext = SynchronizationContext.Current;
            CancelCommand = ReactiveCommand.Create();
        }

        public override void Initialize(IServiceProvider serviceProvider)
        {
            base.Initialize(serviceProvider);

            var disp = ServiceProvider.GetExportedValue<IUIProvider>().GetService<IExportFactoryProvider>().UIControllerFactory.CreateExport();
            disposables.Add(disp);
            uiController = disp.Value;

            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.pullRequestCommand,
                (s, e) => {
                    StartFlow(UIControllerFlow.PullRequests).Forget();
                });

            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.backCommand,
                (s, e) => { });

            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.forwardCommand,
                (s, e) => { });

            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.refreshCommand,
                (s, e) => { });

            StartFlow(UIControllerFlow.PullRequests).Forget();
        }

        void IViewModel.Initialize([AllowNull] object data)
        {
        }

        async Task StartFlow(UIControllerFlow controllerFlow)
        {
            if (uiController != null)
            {
                windowController?.Close();
                uiController.Stop();
                disposables.Clear();
                uiController = null;
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

            var connection = await connectionManager.LookupConnection(ActiveRepo);
            uiController.Start(connection);
        }

        IView control;
        public IView Control
        {
            [return: AllowNull] get { return control; }
            set { control = value; this.RaisePropertyChange(); }
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