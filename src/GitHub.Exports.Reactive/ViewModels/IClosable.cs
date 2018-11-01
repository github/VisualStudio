using System;
using System.Reactive;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Represents an entity that can be closed.
    /// </summary>
    public interface IClosable
    {
        /// <summary>
        /// Gets an observable that is fired when the entity is closed.
        /// </summary>
        IObservable<Unit> Closed { get; }
    }
}
