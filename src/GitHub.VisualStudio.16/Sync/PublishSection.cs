using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Services;
using GitHub.ViewModels.TeamExplorer;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using ReactiveUI;
using Task = System.Threading.Tasks.Task;
using RelayCommand = Microsoft.TeamFoundation.MVVM.RelayCommand;


namespace GitHub.VisualStudio.Sync
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

            PublishToGitHub = new RelayCommand(o => ShowPublishDialogAsync().Forget());
        }

        async Task ShowPublishDialogAsync()
        {
            var exportProvider = compositionServices.GetExportProvider();

            var connectionManager = exportProvider.GetExportedValue<IConnectionManager>();
            var loggedIn = await connectionManager.IsLoggedIn();
            if (!loggedIn)
            {
                var dialogService = exportProvider.GetExportedValue<IDialogService>();
                var connection = await dialogService.ShowLoginDialog();

                if (connection == null)
                {
                    return;
                }
            }

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
            var isVisible = IsSectionVisible(PushToRemoteSectionId);
            if (IsVisible != isVisible)
            {
                if (isVisible)
                {
                    // Initialize the view when it becomes visible
                    EnsureSectionInitialized();
                }

                IsVisible = isVisible;
            }
        }

        void EnsureSectionInitialized()
        {
            if (SectionContent == null)
            {
                // This line also ensures that the GitHub.Resourcess assembly has been loaded before we use it from XAML
                Title = GitHub.Resources.GitHubPublishSectionTitle;
                IsExpanded = true;
                IsBusy = false;

                SectionContent = new PublishView()
                {
                    DataContext = this
                };
            }
        }

        bool IsSectionVisible(Guid sectionId) =>
            ServiceProvider.GetService(typeof(ITeamExplorerPage)) is ITeamExplorerPage page &&
            page.GetSection(sectionId) is ITeamExplorerSection section &&
            section.SectionContent != null &&
            section.IsVisible;
    }
}
