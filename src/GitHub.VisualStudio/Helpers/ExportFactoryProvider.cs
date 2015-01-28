using System.ComponentModel.Composition;

namespace GitHub.Exports
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
        public ExportFactory<ILoginDialog> LoginViewModelFactory { get; set; }
        /*
        [Import(AllowRecomposition = true)]
        public ExportFactory<ITwoFactorDialog> TwoFactorViewModelFactory { get; set; }
        */
    }
}
