using System;
using System.Windows;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using GitHub.Services;
using GitHub.VisualStudio.UI.Services;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using GitHub.Factories;
using GitHub.ViewModels.TeamExplorer;
using GitHub.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel;
using GitHub.Settings;
using Microsoft.VisualStudio.Threading;
using GitHub.VisualStudio.Views.Dialog;
using Microsoft.TeamFoundation.Controls;

namespace GitHub.VisualStudio.TestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            LoginAsync();
        }

        private void CloneOrOpenRepository_Click(object sender, RoutedEventArgs e)
        {
            CloneOrOpenRepositoryAsync();
        }

        private void CreateRepository_Click(object sender, RoutedEventArgs e)
        {
            CreateRepositoryAsync();
        }

        private void Publish_Click(object sender, RoutedEventArgs e)
        {
            PublishAsync();
        }

        async Task PublishAsync()
        {
            var compositionContainer = CreateCompositionContainer();

            var section = CreateTeamExplorerSection(compositionContainer, TeamExplorer.Sync.GitHubPublishSection.GitHubPublishSectionId);
            var view = section.SectionContent;
            //var sp = new MySVsServiceProvider();
            //section.Initialize(this, new Microsoft.TeamFoundation.Controls.SectionInitializeEventArgs(sp, null));

            var window = new ExternalGitHubDialogWindow
            {
                Content = view,
                Owner = this
            };

            window.ShowDialog();
        }

        static ITeamExplorerSection CreateTeamExplorerSection(CompositionContainer compositionContainer, string sectionId)
        {
            var exports = compositionContainer.GetExports<ITeamExplorerSection, IDictionary<string, object>>();
            var export = exports
                .Where(e => e.Metadata[nameof(TeamExplorerSectionAttribute.Id)] as string == sectionId)
                .First();
            var section = export.Value;
            return section;
        }

        async Task LoginAsync()
        {
            var compositionContainer = CreateCompositionContainer();

            var dialogService = compositionContainer.GetExportedValue<IDialogService>();
            await dialogService.ShowLoginDialog();
        }

        async Task CreateRepositoryAsync()
        {
            var compositionContainer = CreateCompositionContainer();

            var dialogService = compositionContainer.GetExportedValue<IDialogService>();
            var connection = await dialogService.ShowLoginDialog();
            if (connection != null)
            {
                await dialogService.ShowCreateRepositoryDialog(connection);
            }
        }

        async Task CloneOrOpenRepositoryAsync()
        {
            var compositionContainer = CreateCompositionContainer();

            var url = null as string;
            var dialogService = compositionContainer.GetExportedValue<IDialogService>();
            var cloneDialogResult = await dialogService.ShowCloneDialog(null, url);
            if (cloneDialogResult != null)
            {
                var repositoryCloneService = compositionContainer.GetExportedValue<IRepositoryCloneService>();
                await repositoryCloneService.CloneOrOpenRepository(cloneDialogResult);
            }
        }

        CompositionContainer CreateCompositionContainer()
        {
            var compositionServices = new CompositionServices();
            var exports = CreateOutOfProcExports();
            var compositionContainer = compositionServices.CreateCompositionContainer(exports);

            var gitHubServiceProvider = compositionContainer.GetExportedValue<IGitHubServiceProvider>();
            var externalShowDialogService = new ExternalShowDialogService(gitHubServiceProvider, this);
            compositionContainer.ComposeExportedValue<IShowDialogService>(externalShowDialogService);
            compositionContainer.ComposeExportedValue<IVSGitServices>(new MyVSGitServices());
            compositionContainer.ComposeExportedValue<IVSGitExt>(new MyVSGitExt());
            compositionContainer.ComposeExportedValue<ITeamExplorerContext>(new MyTeamExplorerContext());
            compositionContainer.ComposeExportedValue<IPackageSettings>(new MyPackageSettings());
            compositionContainer.ComposeExportedValue<ITeamExplorerServiceHolder>(new MyTeamExplorerServiceHolder());
            return compositionContainer;
        }

        static CompositionContainer CreateOutOfProcExports()
        {
            var container = new CompositionContainer();
            var serviceProvider = new MySVsServiceProvider();
            container.ComposeExportedValue<SVsServiceProvider>(serviceProvider);
            return container;
        }

        class MySVsServiceProvider : SVsServiceProvider
        {
            public object GetService(Type serviceType)
            {
                Console.WriteLine($"GetService: {serviceType}");
                return null;
            }
        }

        class MyVSGitServices : IVSGitServices
        {
            public Task Clone(string cloneUrl, string clonePath, bool recurseSubmodules, object progress = null, CancellationToken? cancellationToken = null)
            {
                throw new NotImplementedException();
            }

            public LibGit2Sharp.IRepository GetActiveRepo()
            {
                return null;
            }

            public string GetActiveRepoPath()
            {
                throw new NotImplementedException();
            }

            public IEnumerable<LocalRepositoryModel> GetKnownRepositories()
            {
                throw new NotImplementedException();
            }

            public string GetLocalClonePathFromGitProvider()
            {
                throw new NotImplementedException();
            }

            public string SetDefaultProjectPath(string path)
            {
                throw new NotImplementedException();
            }
        }

        class MyVSGitExt : IVSGitExt
        {
            public IReadOnlyList<LocalRepositoryModel> ActiveRepositories => throw new NotImplementedException();

            public event Action ActiveRepositoriesChanged;

            public void RefreshActiveRepositories()
            {
                throw new NotImplementedException();
            }
        }

        class MyTeamExplorerContext : ITeamExplorerContext
        {
            public LocalRepositoryModel ActiveRepository => throw new NotImplementedException();

            public event EventHandler StatusChanged;
            public event PropertyChangedEventHandler PropertyChanged;
        }

        class MyPackageSettings : Settings.IPackageSettings
        {
            public bool CollectMetrics { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public bool EditorComments { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public UIState UIState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public bool HideTeamExplorerWelcomeMessage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public bool EnableTraceLogging { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public event PropertyChangedEventHandler PropertyChanged;

            public void Save()
            {
                throw new NotImplementedException();
            }
        }

        class MyTeamExplorerServiceHolder : ITeamExplorerServiceHolder
        {
            public IServiceProvider ServiceProvider { get; set; }

            public ITeamExplorerContext TeamExplorerContext => throw new NotImplementedException();

            public JoinableTaskFactory JoinableTaskFactory => throw new NotImplementedException();

            public IGitAwareItem HomeSection => throw new NotImplementedException();

            public void ClearServiceProvider(IServiceProvider provider)
            {
                throw new NotImplementedException();
            }
        }
    }
}
