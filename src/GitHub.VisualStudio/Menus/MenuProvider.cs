using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace GitHub.VisualStudio.Menus
{
    [Export(typeof(IMenuProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MenuProvider : IMenuProvider
    {
        public IReadOnlyCollection<IMenuHandler> Menus { get; }

        public IReadOnlyCollection<IDynamicMenuHandler> DynamicMenus { get; }

        [ImportingConstructor]
        public MenuProvider([ImportMany] IEnumerable<IMenuHandler> menus, [ImportMany] IEnumerable<IDynamicMenuHandler> dynamicMenus)
        {
            Menus = new ReadOnlyCollection<IMenuHandler>(menus.ToList());
            DynamicMenus = new ReadOnlyCollection<IDynamicMenuHandler>(dynamicMenus.ToList());
        }
    }
}
