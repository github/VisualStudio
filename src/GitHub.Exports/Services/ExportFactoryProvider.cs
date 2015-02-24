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
        
        [Import(AllowRecomposition =true)]
        public ExportFactory<ILoginViewModel> LoginViewModelFactory { get; set; }

        [Import(AllowRecomposition = true)]
        public ExportFactory<IUIController> UIControllerFactory { get; set; }
        
        /*
        [Import(AllowRecomposition = true)]
        public ExportFactory<ITwoFactorDialog> TwoFactorViewModelFactory { get; set; }
        */
    }
}
