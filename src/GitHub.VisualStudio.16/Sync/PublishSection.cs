using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using GitHub.Factories;
using GitHub.Primitives;
using GitHub.ViewModels.TeamExplorer;
using GitHub.VisualStudio;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;
using ReactiveUI;


namespace Microsoft.TeamExplorerSample.Sync
{
    /// <summary>
    /// Publish section.
    /// </summary>
    [TeamExplorerSection(SectionId, TeamExplorerPageIds.GitCommits, Priority)]
    public class PublishSection : TeamExplorerBaseSection
    {
        readonly CompositionServices compositionServices;

        public const string SectionId = "35B18474-005D-4A2A-9CCF-FFFFEB60F1F5";
        public const int Priority = 4;

        readonly Guid PushToRemoteSectionId = new Guid("99ADF41C-0022-4C03-B3C2-05047A3F6C2C");

        [ImportingConstructor]
        public PublishSection(CompositionServices compositionServices)
        {
            this.compositionServices = compositionServices;

            this.Title = "Publish to GitHub";
            this.IsExpanded = true;
            this.IsBusy = false;
            this.SectionContent = new PublishView(){
                DataContext = this
            };
            this.View.ParentSection = this;
        }

        /// <summary>
        /// Get the view.
        /// </summary>
        protected PublishView View
        {
            get { return this.SectionContent as PublishView; }
        }

        public ICommand PublishToGitHub { get; set; }

        /// <summary>
        /// Initialize override.
        /// </summary>
        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            base.Initialize(sender, e);

            // Remain hidden when full GitHub extension is installed
            if (FullExtensionUtilities.IsInstalled(ServiceProvider))
            {
                IsVisible = false;
                return;
            }

            RefreshVisibility();

            if (ServiceProvider.GetService(typeof(ITeamExplorerPage)) is ITeamExplorerPage page)
            {
                if (page.GetSection(PushToRemoteSectionId) is ITeamExplorerSection pushToRemoteSection)
                {
                    pushToRemoteSection.PropertyChanged += Section_PropertyChanged;
                }
            }

            PublishToGitHub = new RelayCommand(o => ShowPublishDialog());
        }

        void ShowPublishDialog()
        {
            var exportProvider = compositionServices.GetExportProvider();

            var factory = exportProvider.GetExportedValue<IViewViewModelFactory>();
            var viewModel = exportProvider.GetExportedValue<IRepositoryPublishViewModel>();

            var busy = viewModel.WhenAnyValue(x => x.IsBusy).Subscribe(x => IsBusy = x);
            var completed = viewModel.PublishRepository
                .Where(x => x == ProgressState.Success)
                .Subscribe(_ =>
                {
                    var teamExplorer = ServiceProvider.GetService(typeof(ITeamExplorer)) as ITeamExplorer;
                    teamExplorer?.NavigateToPage(new Guid(TeamExplorerPageIds.Home), null);
                });

            var view = factory.CreateView<IRepositoryPublishViewModel>();
            view.DataContext = viewModel;

            Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                    x => view.Unloaded += x,
                    x => view.Unloaded -= x)
                .Take(1)
                .Subscribe(_ =>
                {
                    busy.Dispose();
                    completed.Dispose();
                });

            SectionContent = view;
        }

        void Section_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ITeamExplorerSection.IsVisible):
                case nameof(ITeamExplorerSection.SectionContent):
                    RefreshVisibility();
                    break;
            }
        }

        void RefreshVisibility()
        {
            var visible = false;

            if (ServiceProvider.GetService(typeof(ITeamExplorerPage)) is ITeamExplorerPage page)
            {
                visible = IsSectionVisible(page, PushToRemoteSectionId);
            }

            if (IsVisible != visible)
            {
                IsVisible = visible;
            }
        }

        bool IsSectionVisible(ITeamExplorerPage teamExplorerPage, Guid sectionId)
        {
            if (teamExplorerPage.GetSection(sectionId) is ITeamExplorerSection pushToRemoteSection)
            {
                return pushToRemoteSection.SectionContent != null && pushToRemoteSection.IsVisible;
            }

            return false;
        }
    }
}
