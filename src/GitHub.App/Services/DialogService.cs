using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.ViewModels.Dialog;
using GitHub.ViewModels.Dialog.Clone;

namespace GitHub.Services
{
    [Export(typeof(IDialogService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DialogService : IDialogService
    {
        readonly IViewViewModelFactory factory;
        readonly IShowDialogService showDialog;
        readonly IGitHubContextService gitHubContextService;

        [ImportingConstructor]
        public DialogService(
            IViewViewModelFactory factory,
            IShowDialogService showDialog,
            IGitHubContextService gitHubContextService)
        {
            Guard.ArgumentNotNull(factory, nameof(factory));
            Guard.ArgumentNotNull(showDialog, nameof(showDialog));
            Guard.ArgumentNotNull(showDialog, nameof(gitHubContextService));

            this.factory = factory;
            this.showDialog = showDialog;
            this.gitHubContextService = gitHubContextService;
        }

        public async Task<CloneDialogResult> ShowCloneDialog(IConnection connection, string url = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                var clipboardContext = gitHubContextService.FindContextFromClipboard();
                url = clipboardContext?.Url;
            }

            var viewModel = factory.CreateViewModel<IRepositoryCloneViewModel>();
            if (url != null)
            {
                viewModel.UrlTab.Url = url;
            }

            if (connection != null)
            {
                return (CloneDialogResult)await showDialog.Show(
                    viewModel,
                    connection,
                    ApiClientConfiguration.RequestedScopes)
                    .ConfigureAwait(false);
            }
            else
            {
                return (CloneDialogResult)await showDialog.ShowWithFirstConnection(viewModel)
                    .ConfigureAwait(false);
            }
        }

        public async Task<string> ShowReCloneDialog(RepositoryModel repository)
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

        public async Task ShowForkDialog(LocalRepositoryModel repository, IConnection connection)
        {
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotNull(connection, nameof(connection));

            var viewModel = factory.CreateViewModel<IForkRepositoryViewModel>();
            await viewModel.InitializeAsync(repository, connection);
            await showDialog.Show(viewModel);
        }
    }
}
