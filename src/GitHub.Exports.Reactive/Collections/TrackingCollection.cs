using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using GitHub.Helpers;
using System.Reactive;
using System.Linq;
using ReactiveUI;

namespace GitHub.Collections
{
    public class TrackingCollection<T> : ObservableCollection<T>, ITrackingCollection<T>
        where T : ICopyable<T>
    {
        enum TheAction
        {
            None,
            Move,
            Add,
            Insert
        }

        CompositeDisposable disposables = new CompositeDisposable();
        IObservable<Unit> source;
        Func<T, T, int> comparer;
        Func<T, int, IList<T>, bool> filter;
        IScheduler scheduler;
        List<T> original;

        public TrackingCollection(Func<T, T, int> comparer = null, Func<T, int, IList<T>, bool> filter = null, IScheduler scheduler = null)
        {
            this.scheduler = scheduler ?? RxApp.MainThreadScheduler;
            this.comparer = comparer;
            this.filter = filter;
            if (filter != null)
                original = new List<T>();
        }

        public TrackingCollection(IObservable<T> source, Func<T, T, int> comparer = null, Func<T, int, IList<T>, bool> filter = null, IScheduler scheduler = null)
            : this(comparer, filter, scheduler)
        {
            Listen(source);
        }

        public ITrackingCollection<T> Listen(IObservable<T> obs)
        {
            source = obs
                .ObserveOn(filter != null ? TaskPoolScheduler.Default : scheduler)
                .Select(t =>
                {
                    var list = filter == null ? this : original as IList<T>;

                    var idx = list.IndexOf(t);
                    if (idx >= 0)
                    {
                        var old = list[idx];
                        var comparison = 0;
                        if (comparer != null)
                            comparison = comparer(t, old);
                        list[idx].CopyFrom(t);

                        if (comparer == null || comparison == 0)
                            return Tuple.Create(TheAction.None, idx, idx); // no sorting to be done, just replacing the element in-place
                        else
                        {
                            var i = idx;
                            if (comparison < 0) // replacing element has lower sorting order, find the correct spot towards the beginning
                                for (var pos = i - 1; i > 0 && comparer(list[i], list[pos]) < 0; i--, pos--)
                                    Swap(list, i, pos);
                            else if (comparison > 0) // replacing element has higher sorting order, find the correct spot towards the end
                                for (var pos = i + 1; i < list.Count - 1 && comparer(list[i], list[pos]) > 0; i++, pos++)
                                    Swap(list, i, pos);

                            if (filter == null)
                            {
                                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, old, i, idx));
                            }
                            return Tuple.Create(TheAction.Move, i, idx); // pass the new position and old position
                        }
                    }
                    // the element doesn't exist yet. If there's a comparer and other items, add it in the correct spot
                    else if (comparer != null && list.Count > 0)
                    {
                        var i = 0;
                        for (; i < list.Count && comparer(t, list[i]) > 0; i++) { }
                        if (i == list.Count)
                        {
                            list.Add(t);
                            return Tuple.Create(TheAction.Add, list.Count - 1, -1);
                        }
                        list.Insert(i, t);
                        return Tuple.Create(TheAction.Insert, i, -1);
                    }
                    else
                    {
                        list.Add(t);
                        return Tuple.Create(TheAction.Add, list.Count - 1, -1);
                    }
                })
                .ObserveOn(scheduler)
                .Select(x =>
                {
                    UpdateFilter(x.Item1, x.Item2, x.Item3);
                    return Unit.Default;
                })
                .Publish()
                .RefCount();
            return this;
        }

        public void SetComparer(Func<T, T, int> compare)
        {
            comparer = compare;
            var list = filter == null ? this : original as IList<T>;
            UpdateSort(list, 0, list.Count);
            UpdateFilter(filter);
        }

        public void SetFilter(Func<T, int, IList<T>, bool> theFilter)
        {
            UpdateFilter(theFilter);
        }

        void UpdateSort(IList<T> list, int start, int end)
        {
            if (comparer == null)
                return;

            for (; start < end - 1; start++)
            {
                var idxSmallest = start;
                for (var i = start + 1; i < end; i++)
                    if (comparer(list[i], list[idxSmallest]) < 0)
                        idxSmallest = i;
                if (idxSmallest == start) continue;
                Swap(list, start, idxSmallest);

                // if there's a filter, then it's going to trigger events and we don't need to manually trigger them
                if (filter == null)
                {
                    OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, this[start], start, idxSmallest));
                }
            }
        }

        void UpdateFilter(Func<T, int, IList<T>, bool> newFilter)
        {
            if (filter == null && newFilter == null)
                return; // nothing to do

            // no more filter, add all the hidden items back
            if (filter != null && newFilter == null)
            {
                for (int i = 0; i < original.Count; i++)
                {
                    if (GetIndexOf(original[i]) < 0)
                        InsertItem(i, original[i]);
                }
                original = null;
                filter = null;
#if !DISABLE_BACKING_DICT
                liveListMap.Clear();
#endif
                return;
            }

            // there was no filter before, so the Items collection has everything, grab it
            if (filter == null)
                original = new List<T>(Items);
            else
            {
                Items.Clear();
#if !DISABLE_BACKING_DICT
                liveListMap.Clear();
#endif
            }
            filter = newFilter;

            RecalculateFilter(0, 0, original.Count);
        }

        void UpdateFilter(TheAction action, int position, int oldPosition)
        {
            if (filter == null)
                return;

            var obj = original[position];
            var isIncluded = filter(obj, position, this);

            // adding is simple, just check if it should be visible and stuff it at the end
            if (action == TheAction.Add)
            {
                if (isIncluded)
                    AddIntoLiveList(obj);
                return;
            }

            // for all cases except add, we need to know where the touched
            // element is going to be located in the live list, so grab that
            var liveListCurrentIndex = GetLiveListPivot(position);

            //if the element was changed but didn't move, then sorting is correct but we 
            //need to check the filter anyway to determine whether to hide or show the element.
            //if that changes, then other elements after it might be affected too, so we need to check
            //all elements from the live list from the element that was changed to the end
            if (action == TheAction.None)
            {
                var idx = GetIndexOf(obj);

                // nothing has changed as far as the live list is concerned
                if ((isIncluded && idx > 0) || !isIncluded && idx < 0)
                    return;

                // wasn't on the live list, but it is now
                if (isIncluded && idx < 0)
                    InsertAndRecalculate(obj, liveListCurrentIndex, position);

                // was on the live list but it isn't anymore
                else if (!isIncluded && idx >= 0)
                    RemoveAndRecalculate(obj, idx, position);

                return;
            }

            if (action == TheAction.Insert)
            {
                if (isIncluded)
                    InsertAndRecalculate(obj, liveListCurrentIndex, position);
                return;
            }

            if (action == TheAction.Move)
            {
                var idx = GetIndexOf(obj);

                // it wasn't included before in the live list, and still isn't, nothing to do
                if ((!isIncluded && idx < 0) || isIncluded && idx == liveListCurrentIndex)
                    return;

                // the move caused the object to not be visible in the live list anymore, so remove
                if (!isIncluded)
                    RemoveAndRecalculate(obj, idx, oldPosition);

                // the move caused the object to become visible in the live list, insert it
                // and recalculate all the other things on the live list after this one
                else if (idx < 0)
                    InsertAndRecalculate(obj, liveListCurrentIndex, position);

                // move the object
                else
                    MoveAndRecalculate(obj, idx, liveListCurrentIndex, oldPosition, position);

                return;
            }
        }

        /// <summary>
        /// Move an object in the live list and recalculate positions
        /// for all objects between the bounds of the affected indexes
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="from">Index in the live list where the object is</param>
        /// <param name="to">Index in the live list where the object is going to be</param>
        /// <param name="fromPosition">Index in the object list (original) corresponding to "from"</param>
        /// <param name="toPosition">Index in the object list (original) corresponding to "to"</param>
        void MoveAndRecalculate(T obj, int from, int to, int fromPosition, int toPosition)
        {
            MoveInLiveList(obj, from, to);
            RecalculateFilter(to, toPosition < fromPosition ? toPosition : fromPosition,
                toPosition < fromPosition ? fromPosition : toPosition);
        }

        /// <summary>
        /// Remove an object from the live list at index and recalculate positions
        /// for all objects after that
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <param name="position"></param>
        void RemoveAndRecalculate(T item, int index, int position)
        {
            RemoveFromLiveList(item, index);
            RecalculateFilter(index, position, original.Count);
        }

        /// <summary>
        /// Insert an object into the live list at liveListCurrentIndex and recalculate
        /// positions for all objects after that
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <param name="position">Index of the object list (original)</param>
        void InsertAndRecalculate(T item, int index, int position)
        {
            InsertIntoLiveList(item, index);
            position++;
            index++;
            RecalculateFilter(index, position, original.Count);
        }

        /// <summary>
        /// Get the index in the live list of an object at position.
        /// This will scan back to the beginning of the live list looking for
        /// the closest left neighbour and return the position after that.
        /// </summary>
        /// <param name="position">The index of an object in the original list that we want to map to the list list</param>
        /// <returns></returns>
        int GetLiveListPivot(int position)
        {
            var liveListCurrentIndex = -1;
            if (position > 0)
            {
                for (int i = position - 1; i >= 0; i--)
                {
                    liveListCurrentIndex = GetIndexOf(original[i]);
                    if (liveListCurrentIndex >= 0)
                    {
                        // found an element to the left of what we want, so now we know the index where to start
                        // manipulating the list
                        liveListCurrentIndex++;
                        break;
                    }
                }
            }

            // there was no element to the left of the one we want, start at the beginning of the live list
            if (liveListCurrentIndex < 0)
                liveListCurrentIndex = 0;
            return liveListCurrentIndex;
        }

        /// <summary>
        /// Go through the list of objects ad adjust their "visibility" in the live list
        /// (by removing/inserting as needed). 
        /// </summary>
        /// <param name="liveListCurrentIndex">Index in the live list corresponding to the start index of the object list</param>
        /// <param name="start">Start index of the object list</param>
        /// <param name="end">End index of the object list</param>
        void RecalculateFilter(int liveListCurrentIndex, int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                var obj = original[i];
                var idx = GetIndexOf(obj);
                var isIncluded = filter(obj, i, this);

                // element is still included and hasn't changed positions
                if (isIncluded && idx >= 0)
                    liveListCurrentIndex++;
                // element is included and wasn't before
                else if (isIncluded && idx < 0)
                {
                    if (liveListCurrentIndex == Count)
                        AddIntoLiveList(obj);
                    else
                        InsertIntoLiveList(obj, liveListCurrentIndex);
                    liveListCurrentIndex++;
                }
                // element is not included and was before
                else if (!isIncluded && idx >= 0)
                    RemoveFromLiveList(obj, idx);
            }
        }

        /// <summary>
        /// Swap two elements
        /// </summary>
        /// <param name="list"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        static void Swap(IList<T> list, int left, int right)
        {
            var l = list[left];
            list[left] = list[right];
            list[right] = l;
        }

#if !DISABLE_BACKING_DICT
        Dictionary<T, int> liveListMap = new Dictionary<T, int>();
#endif

        int GetIndexOf(T item)
        {
#if !DISABLE_BACKING_DICT
            int ret;
            if (liveListMap.TryGetValue(item, out ret))
                return ret;
            return -1;
#else
            return IndexOf(item);
#endif
        }

        void AddIntoLiveList(T item)
        {
#if !DISABLE_BACKING_DICT
            liveListMap.Add(item, Count);
#endif
            Add(item);
        }

        void InsertIntoLiveList(T item, int position)
        {
            InsertItem(position, item);
#if !DISABLE_BACKING_DICT
            liveListMap.Add(item, position);
            var count = Count;
            for (int i = position + 1; i < count; i++)
                liveListMap[this[i]] = i;
#endif
        }

        void MoveInLiveList(T item, int from, int to)
        {
            MoveItem(from, from < to ? to - 1 : to);
#if !DISABLE_BACKING_DICT
            liveListMap.Remove(item);
            var count = to < from ? Count : to;
            for (int i = from < to ? from : to; i < count; i++)
                liveListMap[this[i]] = i;
#endif
        }

        void RemoveFromLiveList(T item, int position)
        {
            RemoveItem(position);
#if !DISABLE_BACKING_DICT
            liveListMap.Remove(item);
            var count = Count;
            for (int i = position; i < count; i++)
                liveListMap[this[i]] = i;
#endif
        }

        public IDisposable Subscribe()
        {
            disposables.Add(source.Subscribe());
            return this;
        }

        public IDisposable Subscribe(Action onCompleted)
        {
            disposables.Add(source.Subscribe(_ => { }, onCompleted));
            return this;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    disposables.Dispose();
                    disposed = true;
                }
            }
        }
    }
}
