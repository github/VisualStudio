using System;
using System.Reactive;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Represents a view model that has a "Done" signal.
    /// </summary>
    public interface IHasDone
    {
        /// <summary>
        /// Gets an observable which will emit a value when the view model is done.
        /// </summary>
        IObservable<Unit> Done { get; }
    }
}
