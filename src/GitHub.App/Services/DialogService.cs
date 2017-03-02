using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.UI;
using GitHub.ViewModels;
using NullGuard;

namespace GitHub.Services
{
    [Export(typeof(IDialogService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DialogService : IDialogService
    {
        readonly IUIProvider uiProvider;

        [ImportingConstructor]
        public DialogService(IUIProvider uiProvider)
        {
            this.uiProvider = uiProvider;
        }

        public Task<CloneDialogResult> ShowCloneDialog([AllowNull] IConnection connection)
        {
            var controller = uiProvider.Configure(UIControllerFlow.Clone, connection);
            var basePath = default(string);
            var repository = default(IRepositoryModel);

            controller.TransitionSignal.Subscribe(x =>
            {
                var vm = x.View.ViewModel as IBaseCloneViewModel;

                ((IDialogView)x.View).Done.Subscribe(_ =>
                {
                    basePath = vm?.BaseRepositoryPath;
                    repository = vm?.SelectedRepository;
                });
            });

            uiProvider.RunInDialog(controller);

            var result = repository != null && basePath != null ?
                new CloneDialogResult(basePath, repository) : null;
            return Task.FromResult(result);
        }

        public Task<string> ShowReCloneDialog(IRepositoryModel repository)
        {
            var controller = uiProvider.Configure(UIControllerFlow.ReClone);
            var basePath = default(string);

            controller.TransitionSignal.Subscribe(x =>
            {
                var vm = x.View.ViewModel as IBaseCloneViewModel;

                if (vm != null)
                {
                    vm.SelectedRepository = repository;
                }

                ((IDialogView)x.View).Done.Subscribe(_ =>
                {
                    basePath = vm?.BaseRepositoryPath;
                });
            });

            uiProvider.RunInDialog(controller);

            return Task.FromResult(basePath);
        }
    }
}
