using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
