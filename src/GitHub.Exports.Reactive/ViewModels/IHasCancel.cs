using System;
using System.Reactive;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Represents a view model that has a "Cancel" signal.
    /// </summary>
    public interface IHasCancel
    {
        /// <summary>
        /// Gets an observable which will emit a value when the view model is cancelled.
        /// </summary>
        IObservable<Unit> Cancel { get; }
    }
}
