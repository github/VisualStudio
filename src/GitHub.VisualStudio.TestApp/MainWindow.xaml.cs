using System;
using System.Windows;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using GitHub.Services;
using GitHub.VisualStudio.UI.Services;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

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
            var exportProvider = compositionServices.GetExportProvider();

            var compositionContainer = new CompositionContainer(exportProvider);
            var gitHubServiceProvider = exportProvider.GetExportedValue<IGitHubServiceProvider>();
            var externalShowDialogService = new ExternalShowDialogService(gitHubServiceProvider, this);
            compositionContainer.ComposeExportedValue<IShowDialogService>(externalShowDialogService);

            return compositionContainer;
        }
    }
}
