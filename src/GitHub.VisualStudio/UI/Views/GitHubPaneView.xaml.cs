using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.Helpers;
using NullGuard;
using GitHub.ViewModels;
using System.Windows.Input;
using ReactiveUI;

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
        }
    }

    [ExportViewModel(ViewType = UIViewType.GitHubPane)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubPaneViewModel : TeamExplorerSectionBase,  IGitHubPaneViewModel
    {
        CompositeDisposable disposables = new CompositeDisposable();

        [ImportingConstructor]
        public GitHubPaneViewModel(ITeamExplorerServiceHolder holder, IConnectionManager cm)
            : base(holder, cm)
        {
        }

        public override void Initialize(IServiceProvider serviceProvider)
        {
            base.Initialize(serviceProvider);
            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.pullRequestCommand, (s, e) => MoveToPage(UIControllerFlow.PullRequests));
        }

        void MoveToPage(UIControllerFlow type)
        {
            disposables.Clear();

            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            var factory = uiProvider.GetService<IExportFactoryProvider>();
            var uiflow = factory.UIControllerFactory.CreateExport();
            disposables.Add(uiflow);
            var ui = uiflow.Value;
            var creation = ui.SelectFlow(type);
            creation.Subscribe(c =>
            {
                disposables.Add(c);
                Controls.Add(c);
            });

            var activeRepo = uiProvider.GetService<ITeamExplorerServiceHolder>().ActiveRepo;
            if (activeRepo != null)
            {
                var conn = connectionManager.Connections.FirstOrDefault(c => c.HostAddress.Equals(HostAddress.Create(activeRepo.CloneUrl)));
                ui.Start(conn);
            }
            else
                ui.Start(null);
        }

        ObservableCollection<IView> controls;
        public ObservableCollection<IView> Controls
        {
            [return: AllowNull] get { return controls; }
            set { controls = value; this.RaisePropertyChange(); }
        }

        public ReactiveCommand<object> CancelCommand { get; private set; }
        public ICommand Cancel { get { return CancelCommand; } }

        public bool IsShowing { get { return true; } }

        bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    controls.Clear();
                    disposables.Dispose();
                    disposed = true;
                }
            }
            base.Dispose(disposing);
        }
    }
}