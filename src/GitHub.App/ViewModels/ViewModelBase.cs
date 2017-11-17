using System;
using GitHub.UI;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Base class for view models.
    /// </summary>
    public abstract class ViewModelBase : ReactiveObject, IViewModel
    {
        /// <inheritdoc/>
        public virtual void Initialize(ViewWithData data)
        {
        }
    }
}
