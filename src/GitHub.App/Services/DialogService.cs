using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using EnvDTE;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.ViewModels.Dialog;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Services
{
    [Export(typeof(IGitHubPaneService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubPaneService : IGitHubPaneService
    {
        private readonly Lazy<DTE> dte;

        [ImportingConstructor]
        public GitHubPaneService(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

            dte = new Lazy<DTE>(() => (DTE)serviceProvider.GetService(typeof(DTE)));
        }

        public string GetPathOfActiveDocument()
        {
            return dte.Value.ActiveDocument.FullName;
        }
    }

    public interface IGitHubPaneService
    {
        string GetPathOfActiveDocument();
    }

    [Export(typeof(IDialogService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DialogService : IDialogService
    {
        readonly IViewViewModelFactory factory;
        readonly IShowDialogService showDialog;

        [ImportingConstructor]
        public DialogService(
            IViewViewModelFactory factory,
            IShowDialogService showDialog)
        {
            Guard.ArgumentNotNull(factory, nameof(factory));
            Guard.ArgumentNotNull(showDialog, nameof(showDialog));

            this.factory = factory;
            this.showDialog = showDialog;
        }

        public async Task<CloneDialogResult> ShowCloneDialog(IConnection connection)
        {
            var viewModel = factory.CreateViewModel<IRepositoryCloneViewModel>();

            if (connection != null)
            {
                await viewModel.InitializeAsync(connection);
                return (CloneDialogResult)await showDialog.Show(viewModel);
            }
            else
            {
                return (CloneDialogResult)await showDialog.ShowWithFirstConnection(viewModel);
            }
        }

        public async Task<string> ShowReCloneDialog(IRepositoryModel repository)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));

            var viewModel = factory.CreateViewModel<IRepositoryRecloneViewModel>();
            viewModel.SelectedRepository = repository;
            return (string)await showDialog.ShowWithFirstConnection(viewModel);
        }

        public async Task ShowCreateGist(IConnection connection)
        {
            var viewModel = factory.CreateViewModel<IGistCreationViewModel>();

            if (connection != null)
            {
                await viewModel.InitializeAsync(connection);
                await showDialog.Show(viewModel);
            }
            else
            {
                await showDialog.ShowWithFirstConnection(viewModel);
            }
        }

        public async Task ShowCreateRepositoryDialog(IConnection connection)
        {
            Guard.ArgumentNotNull(connection, nameof(connection));

            var viewModel = factory.CreateViewModel<IRepositoryCreationViewModel>();
            await viewModel.InitializeAsync(connection);
            await showDialog.Show(viewModel);
        }

        public async Task<IConnection> ShowLoginDialog()
        {
            var viewModel = factory.CreateViewModel<ILoginViewModel>();
            return (IConnection)await showDialog.Show(viewModel);
        }

        public async Task ShowForkDialog(ILocalRepositoryModel repository, IConnection connection)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotNull(connection, nameof(connection));

            var viewModel = factory.CreateViewModel<IForkRepositoryViewModel>();
            await viewModel.InitializeAsync(repository, connection);
            await showDialog.Show(viewModel);
        }
    }
}
