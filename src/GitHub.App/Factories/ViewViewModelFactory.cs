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
    /// <summary>
    /// Factory for creating views and view models.
    /// </summary>
    [Export(typeof(IViewViewModelFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ViewViewModelFactory : IViewViewModelFactory
    {
        readonly IGitHubServiceProvider serviceProvider;

        [ImportingConstructor]
        public ViewViewModelFactory(
            IGitHubServiceProvider serviceProvider,
            ICompositionService cc)
        {
            this.serviceProvider = serviceProvider;
            cc.SatisfyImportsOnce(this);
        }

        [ImportMany(AllowRecomposition = true)]
        IEnumerable<ExportFactory<FrameworkElement, IViewModelMetadata>> Views { get; set; }

        /// <inheritdoc/>
        public TViewModel CreateViewModel<TViewModel>() where TViewModel : IViewModel
        {
            return serviceProvider.ExportProvider.GetExport<TViewModel>().Value;
        }

        /// <inheritdoc/>
        public FrameworkElement CreateView<TViewModel>() where TViewModel : IViewModel
        {
            return CreateView(typeof(TViewModel));
        }

        /// <inheritdoc/>
        public FrameworkElement CreateView(Type viewModel)
        {
            var f = Views.FirstOrDefault(x => x.Metadata.ViewModelType.Contains(viewModel));
            return f?.CreateExport().Value;
        }
    }
}
