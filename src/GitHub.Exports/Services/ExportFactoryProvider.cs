using System;
using System.Diagnostics;
using System.Globalization;
using GitHub.UI;
using System.ComponentModel.Composition;

namespace GitHub.Services
{
    [Export]
    public class ExportFactoryProvider
    {
        [ImportingConstructor]
        public ExportFactoryProvider(ICompositionService cc)
        {
            cc.SatisfyImportsOnce(this);
        }
        
        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<ExportFactory<IViewModel, IViewModelMetadata>> ViewModelFactory { get; set; }

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<ExportFactory<IViewFor, IViewModelMetadata>> ViewFactory { get; set; }

        public ExportLifetimeContext<IViewModel> GetViewModel(UIViewType viewType)
        {
            var f = ViewModelFactory.FirstOrDefault(x => x.Metadata.ViewType == viewType);
            Debug.Assert(f != null, string.Format("Could not locate view model for {0}.", viewtype));
            return f.CreateExport();
        }

        public ExportLifetimeContext<IViewFor> GetView(UIViewType viewType)
        {
            var f = ViewFactory.FirstOrDefault(x => x.Metadata.ViewType == viewType);
            Debug.Assert(f != null, string.Format("Could not locate view for {0}.", viewtype));
            return f.CreateExport();
        }
    }
}
