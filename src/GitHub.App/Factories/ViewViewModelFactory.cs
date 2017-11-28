using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using GitHub.Exports;
using GitHub.Services;
using GitHub.ViewModels;

namespace GitHub.Factories
{
    [Export(typeof(IViewViewModelFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ViewViewModelFactory : IViewViewModelFactory
    {
        readonly IGitHubServiceProvider serviceProvider;
        [ImportMany(AllowRecomposition = true)]
        IEnumerable<ExportFactory<FrameworkElement, IViewModelMetadata>> views { get; set; }

        [ImportingConstructor]
        public ViewViewModelFactory(
            IGitHubServiceProvider serviceProvider,
            ICompositionService cc)
        {
            this.serviceProvider = serviceProvider;
            cc.SatisfyImportsOnce(this);
        }

        public TViewModel CreateViewModel<TViewModel>() where TViewModel : IViewModel
        {
            return serviceProvider.ExportProvider.GetExport<TViewModel>().Value;
        }

        public FrameworkElement CreateView<TViewModel>() where TViewModel : IViewModel
        {
            return CreateView(typeof(TViewModel));
        }

        public FrameworkElement CreateView(Type viewModel)
        {
            var f = views.FirstOrDefault(x => x.Metadata.ViewModelType == viewModel);
            return f?.CreateExport().Value;
        }
    }
}
