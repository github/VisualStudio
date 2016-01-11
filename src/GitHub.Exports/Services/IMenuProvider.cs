using System;
using System.Collections.Generic;

namespace GitHub.VisualStudio
{
    /// <summary>
    /// Container for static and dynamic visibility menus (context, toolbar, top, etc)
    /// Get a reference to this via MEF and register the menus
    /// </summary>
    public interface IMenuProvider
    {
        /// <summary>
        /// Registered via AddTopLevelMenuItem
        /// </summary>
        IEnumerable<IMenuHandler> Menus { get; set; }

        /// <summary>
        /// Registered via AddDynamicMenuItem
        /// </summary>
        IEnumerable<IDynamicMenuHandler> DynamicMenus { get; set; }
    }
}
