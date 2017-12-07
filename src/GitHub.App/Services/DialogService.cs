using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.ViewModels.Dialog;

namespace GitHub.Services
{
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

        public async Task ShowCreateGist()
        {
            var viewModel = factory.CreateViewModel<IGistCreationViewModel>();
            await showDialog.ShowWithFirstConnection(viewModel);
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
    }
}
