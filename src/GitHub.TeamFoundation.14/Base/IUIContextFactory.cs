using System;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio.Base
{
    /// <summary>
    /// Interface to wrap the <see cref="UIContext.FromUIContextGuid"/> method for unit testing.
    /// </summary>
    public interface IUIContextFactory
    {
        /// <summary>
        /// Constructs a Microsoft.VisualStudio.Shell.UIContext instance identified with the given
        /// GUID.
        /// </summary>
        /// <param name="contextGuid">GUID of the UIContext.</param>
        /// <returns>The constructed UIContext instance.</returns>
        IUIContextWrapper FromUIContextGuid(Guid contextGuid);
    }
}
