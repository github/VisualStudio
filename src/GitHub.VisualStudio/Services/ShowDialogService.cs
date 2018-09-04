using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog;
using GitHub.VisualStudio.Views.Dialog;

namespace GitHub.VisualStudio.UI.Services
{
    [Export(typeof(IShowDialogService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShowDialogService : IShowDialogService
    {
        readonly IGitHubServiceProvider serviceProvider;

        [ImportingConstructor]
        public ShowDialogService(IGitHubServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task<object> Show(IDialogContentViewModel viewModel)
        {
            var result = default(object);

            using (var dialogViewModel = CreateViewModel())
            using (dialogViewModel.Done.Take(1).Subscribe(x => result = x))
            {
                dialogViewModel.Start(viewModel);

                var window = new GitHubDialogWindow(dialogViewModel);
                window.ShowModal();
            }

            return Task.FromResult(result);
        }

        public async Task<object> Show<TViewModel>(
            TViewModel viewModel,
            IConnection connection,
            IEnumerable<string> scopes)
                where TViewModel : IDialogContentViewModel, IConnectionInitializedViewModel
        {
            var result = default(object);

            using (var dialogViewModel = CreateViewModel())
            using (dialogViewModel.Done.Take(1).Subscribe(x => result = x))
            {
                if (!connection.Scopes.Matches(scopes))
                {
                    await dialogViewModel.StartWithLogout(viewModel, connection);
                }
                else
                {
                    await viewModel.InitializeAsync(connection);
                    dialogViewModel.Start(viewModel);
                }

                var window = new GitHubDialogWindow(dialogViewModel);
                window.ShowModal();
            }

            return result;
        }

        public async Task<object> ShowWithFirstConnection<TViewModel>(TViewModel viewModel)
            where TViewModel : IDialogContentViewModel, IConnectionInitializedViewModel
        {
            var result = default(object);

            using (var dialogViewModel = CreateViewModel())
            using (dialogViewModel.Done.Take(1).Subscribe(x => result = x))
            {
                await dialogViewModel.StartWithConnection(viewModel);

                var window = new GitHubDialogWindow(dialogViewModel);
                window.ShowModal();
            }

            return result;
        }

        IGitHubDialogWindowViewModel CreateViewModel()
        {
            return serviceProvider.GetService<IGitHubDialogWindowViewModel>();
        }
    }
}
