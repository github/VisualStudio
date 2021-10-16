using System;
using GitHub.UI;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Base class for view models.
    /// </summary>
    /// <remarks>
    /// A view model must inherit from this class in order for a view to be automatically
    /// found by the ViewLocator.
    /// </remarks>
    public abstract class ViewModelBase : ReactiveObject, IViewModel
    {
    }
}
