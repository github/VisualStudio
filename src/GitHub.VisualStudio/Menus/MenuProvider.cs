using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio.Menus
{
    [Export(typeof(IMenuProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MenuProvider : IMenuProvider
    {
        [ImportMany]
        public IEnumerable<IMenuHandler> Menus { get; set; }

        [ImportMany]
        public IEnumerable<IDynamicMenuHandler> DynamicMenus { get; set; }
    }
}
