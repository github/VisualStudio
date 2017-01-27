using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GitHub.VisualStudio
{
    /// <summary>
    /// Container for static and dynamic visibility menus (context, toolbar, top, etc)
    /// Get a reference to this via MEF and register the menus
    /// </summary>
    [Guid(Guids.MenuProviderId)]
    public interface IMenuProvider
    {
        /// <summary>
        /// Registered via AddCommandHandler
        /// </summary>
        IReadOnlyCollection<IMenuHandler> Menus { get; }

        /// <summary>
        /// Registered via AddCommandHandler
        /// </summary>
        IReadOnlyCollection<IDynamicMenuHandler> DynamicMenus { get; }
    }
}
