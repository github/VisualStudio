#pragma warning disable 169
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Disposables;
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
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Controls;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericGitHubPaneView : SimpleViewUserControl<IGitHubPaneViewModel, GitHubPaneView>
    {
    }

    [ExportView(ViewType = UIViewType.GitHubPane)]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public partial class GitHubPaneView : GenericGitHubPaneView
    {
        public GitHubPaneView()
        {
            this.InitializeComponent();
            DataContextChanged += (s, e) => ViewModel = e.NewValue as GitHubPaneViewModel;
            this.WhenActivated(d =>
            {
                //d(this.OneWayBind(ViewModel, vm => vm.Controls, v => v.container.ItemsSource));
                //d(this.OneWayBind(ViewModel, vm => vm.Control, v => v.container.ItemsSource));
            });
        }
    }

    [ExportViewModel(ViewType = UIViewType.GitHubPane)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubPaneViewModel : TeamExplorerSectionBase, IGitHubPaneViewModel
    {
        CompositeDisposable disposables = new CompositeDisposable();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        IUIController uiController;

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
        }

        async Task StartFlow(UIControllerFlow controllerFlow)
        {
            if (uiController != null)
            {
                uiController.Stop();
                disposables.Clear();
                uiController = null;
            }

            WindowController windowController = null;
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            var factory = uiProvider.GetService<IExportFactoryProvider>();
            var uiflow = factory.UIControllerFactory.CreateExport();
            disposables.Add(uiflow);
            uiController = uiflow.Value;
            var creation = uiController.SelectFlow(controllerFlow);
            creation
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Subscribe(c =>
                {
                    if (uiController.CurrentFlow == UIControllerFlow.Authentication)
                    {
                        syncContext.Post(_ =>
                        {
                            windowController = new WindowController(creation);
                            windowController.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                            Control = c;
                            windowController.ShowModal();
                        }, null);
                    }
                    else
                    {
                        syncContext.Post(_ =>
                        {
                            Control = c;
                        }, null);
                    }
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