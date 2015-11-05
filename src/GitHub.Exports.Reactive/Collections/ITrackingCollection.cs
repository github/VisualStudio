using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace GitHub.Collections
{
    /// <summary>
    /// TrackingCollection is a specialization of ObservableCollection that gets items from
    /// an observable sequence and updates its contents in such a way that two updates to
    /// the same object (as defined by an Equals call) will result in one object on
    /// the list being updated (as opposed to having two different instances of the object
    /// added to the list).
    /// It is always sorted, either via the supplied comparer or using the default comparer
    /// for T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITrackingCollection<T> : IDisposable, IList<T> where T : ICopyable<T>
    {
        /// <summary>
        /// Sets up an observable as source for the collection.
        /// </summary>
        /// <param name="obs"></param>
        /// <returns>An observable that will return all the items that are
        /// fed via the original observer, for further processing by user code
        /// if desired</returns>
        IObservable<T> Listen(IObservable<T> obs);
        IDisposable Subscribe();
        IDisposable Subscribe(Action<T> onNext, Action onCompleted);
        /// <summary>
        /// Set a new comparer for the existing data. This will cause the
        /// collection to be resorted and refiltered.
        /// </summary>
        /// <param name="theComparer">The comparer method for sorting, or null if not sorting</param>
        void SetComparer(Func<T, T, int> comparer);
        /// <summary>
        /// Set a new filter. This will cause the collection to be filtered
        /// </summary>
        /// <param name="theFilter">The new filter, or null to not have any filtering</param>
        void SetFilter(Func<T, int, IList<T>, bool> filter);
        void AddItem(T item);
        void RemoveItem(T item);
        /// <summary>
        /// How long to delay between processing incoming items
        /// </summary>
        TimeSpan ProcessingDelay { get; set; }
        event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}