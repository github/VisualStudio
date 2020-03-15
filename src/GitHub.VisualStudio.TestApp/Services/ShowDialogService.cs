using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog;
using GitHub.VisualStudio.Views.Dialog;

namespace GitHub.VisualStudio.UI.Services
{
    [Export(typeof(IShowDialogService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ExternalShowDialogService : IShowDialogService
    {
        readonly IGitHubServiceProvider serviceProvider;
        readonly Window ownerWindow;

        [ImportingConstructor]
        public ExternalShowDialogService(IGitHubServiceProvider serviceProvider, Window ownerWindow)
        {
            this.serviceProvider = serviceProvider;
            this.ownerWindow = ownerWindow;
        }

        public Task<object> Show(IDialogContentViewModel viewModel)
        {
            var result = default(object);

            using (var dialogViewModel = CreateViewModel())
            using (dialogViewModel.Done.Take(1).Subscribe(x => result = x))
            {
                dialogViewModel.Start(viewModel);

                var window = CreateWindow(dialogViewModel);
                window.ShowDialog();
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

                var window = CreateWindow(dialogViewModel);
                window.ShowDialog();
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
                var task = dialogViewModel.StartWithConnection(viewModel);
                var window = CreateWindow(dialogViewModel);
                window.ShowDialog();
                await task;
            }

            return result;
        }

        IGitHubDialogWindowViewModel CreateViewModel()
        {
            return serviceProvider.GetService<IGitHubDialogWindowViewModel>();
        }

        Window CreateWindow(IGitHubDialogWindowViewModel dialogViewModel)
        {
            return new ExternalGitHubDialogWindow(dialogViewModel)
            {
                Owner = ownerWindow
            };
        }
    }
}
