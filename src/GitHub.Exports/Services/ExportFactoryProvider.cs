using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using GitHub.Models;

namespace GitHub.Services
{
    [Export(typeof(IExportFactoryProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ExportFactoryProvider : IExportFactoryProvider
    {
        [ImportingConstructor]
        public ExportFactoryProvider(ICompositionService cc)
        {
            cc.SatisfyImportsOnce(this);
        }

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<ExportFactory<IViewModel, IViewModelMetadata>> ViewModelFactory { get; set; }

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<ExportFactory<IView, IViewModelMetadata>> ViewFactory { get; set; }

        public ExportLifetimeContext<IViewModel> GetViewModel(UIViewType viewType)
        {
            Debug.Assert(ViewModelFactory != null, "Attempted to obtain a view model before we imported the ViewModelFactory");
            var f = ViewModelFactory.FirstOrDefault(x => x.Metadata.ViewType == viewType);
            Debug.Assert(f != null, string.Format(CultureInfo.InvariantCulture, "Could not locate view model for {0}.", viewType));
            return f.CreateExport();
        }

        public ExportLifetimeContext<IView> GetView(UIViewType viewType)
        {
            var f = ViewFactory.FirstOrDefault(x => x.Metadata.ViewType == viewType);
            Debug.Assert(f != null, string.Format(CultureInfo.InvariantCulture, "Could not locate view for {0}.", viewType));
            return f.CreateExport();
        }
    }
}
