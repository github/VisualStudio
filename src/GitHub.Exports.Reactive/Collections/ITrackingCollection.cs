using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive;

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
    public interface ITrackingCollection<T> : IDisposable,
        INotifyCollectionChanged, INotifyPropertyChanged,
        IList<T>, ICollection<T>, IEnumerable<T>
        where T : class, ICopyable<T>
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
        Func<T, T, int> Comparer { get; set; }

        /// <summary>
        /// Set a new filter. This will cause the collection to be filtered
        /// </summary>
        /// <param name="theFilter">The new filter, or null to not have any filtering</param>
        Func<T, int, IList<T>, bool> Filter { get; set; }

        /// <summary>
        /// Set a comparer that determines whether the item being processed is newer than the same
        /// item seen before. This is to prevent stale items from overriding newer items when data
        /// is coming simultaneously from cache and from live data. Use a timestamp-like comparison
        /// for best results
        /// </summary>
        /// <param name="theComparer">The comparer method for sorting, or null if not sorting</param>
        Func<T, T, int> NewerComparer { get; set; }

        void AddItem(T item);
        void RemoveItem(T item);
        /// <summary>
        /// How long to delay between processing incoming items
        /// </summary>
        TimeSpan ProcessingDelay { get; set; }
        IObservable<Unit> OriginalCompleted { get; }

        /// <summary>
        /// Returns the number of elements that the collection contains
        /// regardless of filtering
        /// </summary>
        int UnfilteredCount { get; }

        bool Disposed { get; }
    }
}