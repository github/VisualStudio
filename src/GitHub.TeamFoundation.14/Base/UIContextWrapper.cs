using System;
using System.ComponentModel.Composition;
using GitHub.Extensions;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio.Base
{
    /// <summary>
    /// Wraps a <see cref="UIContext"/> for unit testing.
    /// </summary>
    [Export(typeof(IUIContextWrapper))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class UIContextWrapper : IUIContextWrapper
    {
        readonly UIContext inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIContextWrapper"/> class.
        /// </summary>
        /// <param name="inner">The <see cref="UIContext"/> to wrap.</param>
        public UIContextWrapper(UIContext inner)
        {
            Guard.ArgumentNotNull(inner, nameof(inner));

            this.inner = inner;
        }

        /// <summary>
        /// Gets the current state of the UI context, whether it is active or not.
        /// </summary>
        public bool IsActive => inner.IsActive;

        /// <summary>
        /// Occurs whenever the UI context becomes active or inactive.
        /// </summary>
        public event EventHandler<UIContextChangedEventArgs> UIContextChanged
        {
            add { inner.UIContextChanged += value; }
            remove { inner.UIContextChanged -= value; }
        }
    }
}
