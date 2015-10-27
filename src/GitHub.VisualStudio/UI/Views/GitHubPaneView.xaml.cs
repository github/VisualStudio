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
    public class GitHubPaneViewModel : TeamExplorerSectionBase, IGitHubPaneViewModel
    {
        CompositeDisposable disposables = new CompositeDisposable();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        IUIController uiController;

        [ImportingConstructor]
        public GitHubPaneViewModel(ITeamExplorerServiceHolder holder, IConnectionManager cm)
            : base(holder, cm)
        {
            Controls = new ObservableCollection<IView>();
        }

        public override void Initialize(IServiceProvider serviceProvider)
        {
            base.Initialize(serviceProvider);

            var disp = ServiceProvider.GetExportedValue<IUIProvider>().GetService<IExportFactoryProvider>().UIControllerFactory.CreateExport();
            disposables.Add(disp);
            uiController = disp.Value;

            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.pullRequestCommand,
                (s, e) => {});

            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.backCommand,
                (s, e) => { });

            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.forwardCommand,
                (s, e) => { });

            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.refreshCommand,
                (s, e) => { });
        }

        ObservableCollection<IView> controls;
        public ObservableCollection<IView> Controls
        {
            [return: AllowNull] get { return controls; }
            set { controls = value; this.RaisePropertyChange(); }
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
                    controls.Clear();
                    disposables.Dispose();
                    disposed = true;
                }
            }
            base.Dispose(disposing);
        }
    }
}