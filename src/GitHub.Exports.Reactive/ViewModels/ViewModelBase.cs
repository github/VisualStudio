using System;
using System.Reactive;
using GitHub.Logging;
using ReactiveUI;
using Serilog;

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
        static readonly ILogger logger = LogManager.ForContext<ViewModelBase>();

        static ViewModelBase()
        {
            // We don't really have a better place to hook this up as we don't want to force-load
            // rx on package load.
            RxApp.DefaultExceptionHandler = Observer.Create<Exception>(
                ex => logger.Error(ex, "Unhandled rxui error"),
                ex => logger.Error(ex, "Unhandled rxui error"));
        }
    }
}
