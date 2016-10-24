using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio.Base
{
    /// <summary>
    /// Wraps the <see cref="UIContext.FromUIContextGuid"/> method for unit testing.
    /// </summary>
    [Export(typeof(IUIContextFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class UIContextFactory : IUIContextFactory
    {
        /// <summary>
        /// Constructs a Microsoft.VisualStudio.Shell.UIContext instance identified with the given
        /// GUID.
        /// </summary>
        /// <param name="contextGuid">GUID of the UIContext.</param>
        /// <returns>The constructed UIContext instance.</returns>
        public IUIContextWrapper FromUIContextGuid(Guid contextGuid)
        {
            return new UIContextWrapper(UIContext.FromUIContextGuid(contextGuid));
        }
    }
}
