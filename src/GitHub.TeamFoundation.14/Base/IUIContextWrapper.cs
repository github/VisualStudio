using System;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio.Base
{
    /// <summary>
    /// Interface to wrap a <see cref="UIContext"/> for unit testing.
    /// </summary>
    public interface IUIContextWrapper
    {
        /// <summary>
        /// Gets the current state of the UI context, whether it is active or not.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Occurs whenever the UI context becomes active or inactive.
        /// </summary>
        event EventHandler<UIContextChangedEventArgs> UIContextChanged;
    }
}
