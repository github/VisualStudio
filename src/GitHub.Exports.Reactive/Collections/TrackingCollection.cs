#if !DISABLE_REACTIVEUI
using ReactiveUI;
#else
using System.Windows.Threading;
using System.Threading;
#endif
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

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
    public class TrackingCollection<T> : ObservableCollection<T>, ITrackingCollection<T>, IDisposable
        where T : class, ICopyable<T>, IComparable<T>
    {
        enum TheAction
        {
            None,
            Move,
            Add,
            Insert,
            Remove
        }

        bool isChanging;

        readonly CompositeDisposable disposables = new CompositeDisposable();
        IObservable<T> source;
        IObservable<T> sourceQueue;
        Func<T, T, int> comparer;
        Func<T, int, IList<T>, bool> filter;
        readonly IScheduler scheduler;
        ConcurrentQueue<ActionData> queue;

        readonly List<T> original = new List<T>();
#if DEBUG
        public IList<T> DebugInternalList => original;
#endif

        // lookup optimizations
        // for speeding up IndexOf in the unfiltered list
        readonly Dictionary<T, int> sortedIndexCache = new Dictionary<T, int>();

        // for speeding up IndexOf in the filtered list
        readonly Dictionary<T, int> filteredIndexCache = new Dictionary<T, int>();

        TimeSpan delay;
        TimeSpan requestedDelay;
        readonly TimeSpan fuzziness;
        public TimeSpan ProcessingDelay
        {
            get { return requestedDelay; }
            set
            {
                requestedDelay = value;
                delay = value;
            }
        }

        public TrackingCollection(Func<T, T, int> comparer = null, Func<T, int, IList<T>, bool> filter = null, IScheduler scheduler = null)
        {
            queue = new ConcurrentQueue<ActionData>();
            ProcessingDelay = TimeSpan.FromMilliseconds(10);
            fuzziness = TimeSpan.FromMilliseconds(1);

#if DISABLE_REACTIVEUI
            this.scheduler = GetScheduler(scheduler);
#else
            this.scheduler = scheduler ?? RxApp.MainThreadScheduler;
#endif
            this.comparer = comparer ?? Comparer<T>.Default.Compare;
            this.filter = filter;
        }

        public TrackingCollection(IObservable<T> source,
            Func<T, T, int> comparer = null,
            Func<T, int, IList<T>, bool> filter = null,
            IScheduler scheduler = null)
            : this(comparer, filter, scheduler)
        {
            Listen(source);
        }

        /// <summary>
        /// Sets up an observable as source for the collection.
        /// </summary>
        /// <param name="obs"></param>
        /// <returns>An observable that will return all the items that are
        /// fed via the original observer, for further processing by user code
        /// if desired</returns>
        public IObservable<T> Listen(IObservable<T> obs)
        {
            if (disposed)
                throw new ObjectDisposedException("TrackingCollection");

            sourceQueue = obs
                .Do(data => queue.Enqueue(new ActionData(data)));

            source = Observable
                .Generate(StartQueue(),
                    i => !disposed,
                    i => i + 1,
                    i => GetFromQueue(),
                    i => delay
                )
                .Where(data => data.Item != null)
                .ObserveOn(scheduler)
                .Select(x => ProcessItem(x, original))
                // if we're removing an item that doesn't exist, ignore it
                .Where(data => !(data.TheAction == TheAction.Remove && data.OldPosition < 0))
                .Select(SortedNone)
                .Select(SortedAdd)
                .Select(SortedInsert)
                .Select(SortedMove)
                .Select(SortedRemove)
                .Select(CheckFilter)
                .Select(FilteredAdd)
                .Select(CalculateIndexes)
                .Select(FilteredNone)
                .Select(FilteredInsert)
                .Select(FilteredMove)
                .Select(FilteredRemove)
                .TimeInterval()
                .Select(UpdateProcessingDelay)
                .Select(data => data.Item)
                .Publish()
                .RefCount();

            return source;
        }

        /// <summary>
        /// Set a new comparer for the existing data. This will cause the
        /// collection to be resorted and refiltered.
        /// </summary>
        /// <param name="theComparer">The comparer method for sorting, or null if not sorting</param>
        public void SetComparer(Func<T, T, int> theComparer)
        {
            if (disposed)
                throw new ObjectDisposedException("TrackingCollection");
            SetAndRecalculateSort(theComparer);
            SetAndRecalculateFilter(filter);
        }

        /// <summary>
        /// Set a new filter. This will cause the collection to be filtered
        /// </summary>
        /// <param name="theFilter">The new filter, or null to not have any filtering</param>
        public void SetFilter(Func<T, int, IList<T>, bool> theFilter)
        {
            if (disposed)
                throw new ObjectDisposedException("TrackingCollection");
            SetAndRecalculateFilter(theFilter);
        }

        public IDisposable Subscribe()
        {
            if (source == null)
                throw new InvalidOperationException("No source observable has been set. Call Listen or pass an observable to the constructor");
            if (disposed)
                throw new ObjectDisposedException("TrackingCollection");
            disposables.Add(source.Subscribe());
            return this;
        }

        public IDisposable Subscribe(Action<T> onNext, Action onCompleted)
        {
            if (source == null)
                throw new InvalidOperationException("No source observable has been set. Call Listen or pass an observable to the constructor");
            if (disposed)
                throw new ObjectDisposedException("TrackingCollection");
            disposables.Add(source.Subscribe(onNext, onCompleted));
            return this;
        }

        public void AddItem(T item)
        {
            if (disposed)
                throw new ObjectDisposedException("TrackingCollection");
            queue.Enqueue(new ActionData(item));
        }

        public void RemoveItem(T item)
        {
            if (disposed)
                throw new ObjectDisposedException("TrackingCollection");
            queue.Enqueue(new ActionData(TheAction.Remove, item));
        }

        void SetAndRecalculateSort(Func<T, T, int> theComparer)
        {
            comparer = theComparer ?? Comparer<T>.Default.Compare;
            RecalculateSort(original, 0, original.Count);
        }

        void RecalculateSort(List<T> list, int start, int end)
        {
            list.Sort(start, end, new LambdaComparer<T>(comparer));
        }

        void SetAndRecalculateFilter(Func<T, int, IList<T>, bool> newFilter)
        {
            ClearItems();
            filter = newFilter;
            RecalculateFilter(original, 0, 0, original.Count, true);
        }

        #region Source pipeline processing

        ActionData CheckFilter(ActionData data)
        {
            var isIncluded = true;
            if (data.TheAction == TheAction.Remove)
                isIncluded = false;
            else if (filter != null)
                isIncluded = filter(data.Item, data.Position, this);
            return new ActionData(data, isIncluded);
        }

        int StartQueue()
        {
            disposables.Add(sourceQueue.Subscribe());
            return 0;
        }

        ActionData GetFromQueue()
        {
            try
            {
                ActionData d = ActionData.Default;
                if (queue?.TryDequeue(out d) ?? false)
                    return d;
            }
            catch { }
            return ActionData.Default;
        }

        ActionData ProcessItem(ActionData data, List<T> list)
        {
            ActionData ret;
            T item = data.Item;

            var idx = GetIndexUnfiltered(item);

            if (data.TheAction == TheAction.Remove)
                return new ActionData(TheAction.Remove, original, item, null, idx - 1, idx);

            if (idx >= 0)
            {
                var old = list[idx];
                var comparison = comparer(item, old);

                // no sorting to be done, just replacing the element in-place
                if (comparison == 0)
                    ret = new ActionData(TheAction.None, list, item, null, idx, idx);
                else
                    // element has moved, save the original object, because we want to update its contents and move it
                    // but not overwrite the instance.
                    ret = new ActionData(TheAction.Move, list, item, old, comparison, idx);
            }
            // the element doesn't exist yet
            // figure out whether we're larger than the last element or smaller than the first or
            // if we have to place the new item somewhere in the middle
            else if (list.Count > 0)
            {
                if (comparer(list[0], item) >= 0)
                    ret = new ActionData(TheAction.Insert, list, item, null, 0, -1);

                else if (comparer(list[list.Count - 1], item) <= 0)
                    ret = new ActionData(TheAction.Add, list, item, null, list.Count, -1);

                // this happens if the original observable is not sorted, or it's sorting order doesn't
                // match the comparer that has been set
                else
                {
                    idx = BinarySearch(list, item, comparer);
                    if (idx < 0)
                        idx = ~idx;
                    ret = new ActionData(TheAction.Insert, list, item, null, idx, -1);
                }
            }
            else
                ret = new ActionData(TheAction.Add, list, item, null, list.Count, -1);
            return ret;
        }

        ActionData SortedNone(ActionData data)
        {
            if (data.TheAction != TheAction.None)
                return data;
            data.List[data.OldPosition].CopyFrom(data.Item);
            return data;
        }

        ActionData SortedAdd(ActionData data)
        {
            if (data.TheAction != TheAction.Add)
                return data;
            data.List.Add(data.Item);
            return data;
        }

        ActionData SortedInsert(ActionData data)
        {
            if (data.TheAction != TheAction.Insert)
                return data;
            data.List.Insert(data.Position, data.Item);
            UpdateIndexCache(data.Position, data.List.Count, data.List, sortedIndexCache);
            return data;
        }
        ActionData SortedMove(ActionData data)
        {
            if (data.TheAction != TheAction.Move)
                return data;
            data.OldItem.CopyFrom(data.Item);
            var pos = FindNewPositionForItem(data.OldPosition, data.Position < 0, data.List, comparer, sortedIndexCache);
            // the old item is the one moving around
            return new ActionData(data, pos);
        }

        ActionData SortedRemove(ActionData data)
        {
            if (data.TheAction != TheAction.Remove)
                return data;

            // unfiltered list update
            sortedIndexCache.Remove(data.Item);
            UpdateIndexCache(data.List.Count - 1, data.OldPosition, data.List, sortedIndexCache);
            original.Remove(data.Item);
            return data;
        }

        ActionData FilteredAdd(ActionData data)
        {
            if (data.TheAction != TheAction.Add)
                return data;

            if (data.IsIncluded)
                InternalAddItem(data.Item);
            return data;
        }

        ActionData CalculateIndexes(ActionData data)
        {
            var index = GetIndexFiltered(data.Item);
            var indexPivot = GetLiveListPivot(data.Position, data.List);
            return new ActionData(data, index, indexPivot);
        }

        ActionData FilteredNone(ActionData data)
        {
            if (data.TheAction != TheAction.None)
                return data;

            // nothing has changed as far as the live list is concerned
            if ((data.IsIncluded && data.Index >= 0) || !data.IsIncluded && data.Index < 0)
                return data;

            // wasn't on the live list, but it is now
            if (data.IsIncluded && data.Index < 0)
                InsertAndRecalculate(data.List, data.Item, data.IndexPivot, data.Position, false);

            // was on the live list, it's not anymore
            else
                RemoveAndRecalculate(data.List, data.Item, data.IndexPivot, data.Position);

            return data;
        }

        ActionData FilteredInsert(ActionData data)
        {
            if (data.TheAction != TheAction.Insert)
                return data;

            if (data.IsIncluded)
                InsertAndRecalculate(data.List, data.Item, data.IndexPivot, data.Position, false);

            // need to recalculate the filter because inserting an object (even if it's not itself visible)
            // can change visibility of other items after it
            else
                RecalculateFilter(data.List, data.IndexPivot, data.Position, data.List.Count);
            return data;
        }

        /// <summary>
        /// Checks if the object being moved affects the filtered list in any way and update
        /// the list accordingly
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        ActionData FilteredMove(ActionData data)
        {
            if (data.TheAction != TheAction.Move)
                return data;

            var start = data.OldPosition < data.Position ? data.OldPosition : data.Position;
            var end = data.Position > data.OldPosition ? data.Position : data.OldPosition;

            // if there's no filter, the filtered list is equal to the unfiltered list, just move
            if (filter == null)
            {
                MoveAndRecalculate(data.List, data.Index, data.IndexPivot, start, end);
                return data;
            }

            var filteredListChanged = false;
            var startPosition = Int32.MaxValue;
            // check if the filtered list is affected indirectly by the move (eg., if the filter involves position of items,
            // moving an item outside the bounds of the filter can affect the items being currently shown/hidden)
            if (Count > 0)
            {
                startPosition = GetIndexUnfiltered(this[0]);
                var endPosition = GetIndexUnfiltered(this[Count - 1]);
                // true if the filtered list has been indirectly affected by this objects' move
                filteredListChanged = (!filter(this[0], startPosition, this) || !filter(this[Count - 1], endPosition, this));
            }

            // the move caused the object to not be visible in the live list anymore, so remove
            if (!data.IsIncluded && data.Index >= 0)
                RemoveAndRecalculate(data.List, data.Item, filteredListChanged ? 0 : (data.Position < data.OldPosition ? data.IndexPivot : data.Index), filteredListChanged ? startPosition : start);

            // the move caused the object to become visible in the live list, insert it
            // and recalculate all the other things on the live list from the start position
            else if (data.IsIncluded && data.Index < 0)
            {
                start = startPosition < start ? startPosition : start;
                InsertAndRecalculate(data.List, data.Item, data.IndexPivot, start, filteredListChanged);
            }

            // move the object and recalculate the filter between the bounds of the move
            else if (data.IsIncluded)
                MoveAndRecalculate(data.List, data.Index, data.IndexPivot, start, end);

            // recalculate the filter for every item, there's no way of telling what changed
            else if (filteredListChanged)
                RecalculateFilter(data.List, 0, 0, data.List.Count);

            return data;
        }

        /// <summary>
        /// Checks if the object being moved affects the filtered list in any way and update
        /// the list accordingly
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        ActionData FilteredRemove(ActionData data)
        {
            if (data.TheAction != TheAction.Remove)
                return data;

            var filteredListChanged = false;
            var startPosition = Int32.MaxValue;
            // check if the filtered list is affected indirectly by the move (eg., if the filter involves position of items,
            // removing an item outside the bounds of the filter can affect the items being currently shown/hidden)
            if (filter != null && Count > 0)
            {
                startPosition = GetIndexUnfiltered(this[0]);
                var endPosition = GetIndexUnfiltered(this[Count - 1]);
                // true if the filtered list has been indirectly affected by this objects' removal
                filteredListChanged = (!filter(this[0], startPosition, this) || !filter(this[Count - 1], endPosition, this));
            }

            // remove the object if it was visible in the first place
            if (data.Index >= 0)
                RemoveAndRecalculate(data.List, data.Item, filteredListChanged ? 0 : data.IndexPivot, filteredListChanged ? startPosition : data.Position);

            // recalculate the filter for every item, there's no way of telling what changed
            else if (filteredListChanged)
                RecalculateFilter(data.List, 0, 0, data.List.Count);

            return data;
        }

        /// <summary>
        /// Compensate time between items by time taken in processing them
        /// so that the average time between an item being processed
        /// is +- the requested processing delay.
        /// </summary>
        ActionData UpdateProcessingDelay(TimeInterval<ActionData> data)
        {
            if (requestedDelay == TimeSpan.Zero)
                return data.Value;
            var time = data.Interval;
            if (time > requestedDelay + fuzziness)
                delay -= time - requestedDelay;
            else if (time < requestedDelay + fuzziness)
                delay += requestedDelay - time;
            delay = delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
            return data.Value;
        }

        #endregion

        /// <summary>
        /// Insert an object into the live list at liveListCurrentIndex and recalculate
        /// positions for all objects from the position
        /// </summary>
        /// <param name="list">The unfiltered, sorted list of items</param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <param name="position">Index of the unfiltered, sorted list to start reevaluating the filtered list</param>
        /// <param name="rescanAll">Whether the whole filtered list needs to be reevaluated</param>
        void InsertAndRecalculate(IList<T> list, T item, int index, int position, bool rescanAll)
        {
            InternalInsertItem(item, index);
            if (rescanAll)
                index = 0; // reevaluate filter from the start of the filtered list
            else
            {
                // if the item in position is different from the item we're inserting,
                // that means that this insertion might require some filter reevaluation of items
                // before the one we're inserting. We need to figure out if the item in position
                // is in the filtered list, and if it is, then that's where we need to start
                // reevaluating the filter. If it isn't, then there's no need to reevaluate from
                // there
                var needToBacktrack = false;
                if (!Equals(item, list[position]))
                {
                    var idx = GetIndexFiltered(list[position]);
                    if (idx >= 0)
                    {
                        needToBacktrack = true;
                        index = idx;
                    }
                }

                if (!needToBacktrack)
                {
                    index++;
                    position++;
                }
            }
            RecalculateFilter(list, index, position, list.Count);
        }

        /// <summary>
        /// Remove an object from the live list at index and recalculate positions
        /// for all objects after that
        /// </summary>
        /// <param name="list">The unfiltered, sorted list of items</param>
        /// <param name="item"></param>
        /// <param name="index">The index in the live list</param>
        /// <param name="position">The position in the sorted, unfiltered list</param>
        void RemoveAndRecalculate(IList<T> list, T item, int index, int position)
        {
            InternalRemoveItem(item);
            RecalculateFilter(list, index, position, list.Count);
        }

        /// <summary>
        /// Move an object in the live list and recalculate positions
        /// for all objects between the bounds of the affected indexes
        /// </summary>
        /// <param name="list">The unfiltered, sorted list of items</param>
        /// <param name="from">Index in the live list where the object is</param>
        /// <param name="to">Index in the live list where the object is going to be</param>
        /// <param name="start">Index in the unfiltered, sorted list to start reevaluating the filter</param>
        /// <param name="end">Index in the unfiltered, sorted list to end reevaluating the filter</param>
        void MoveAndRecalculate(IList<T> list, int from, int to, int start, int end)
        {
            if (start > end)
                throw new ArgumentOutOfRangeException(nameof(start), "Start cannot be bigger than end, evaluation of the filter goes forward.");

            InternalMoveItem(from, to);
            start++;
            RecalculateFilter(list, (from < to ? from : to) + 1, start, end);
        }


        /// <summary>
        /// Go through the list of objects and adjust their "visibility" in the live list
        /// (by removing/inserting as needed). 
        /// </summary>
        /// <param name="list">The unfiltered, sorted list of items</param>
        /// <param name="index">Index in the live list corresponding to the start index of the object list</param>
        /// <param name="start">Start index of the object list</param>
        /// <param name="end">End index of the object list</param>
        /// <param name="force">If there's no filter set, this method does nothing. Pass true to force a reevaluation
        /// of the whole list regardless of filter.</param>
        void RecalculateFilter(IList<T> list, int index, int start, int end, bool force = false)
        {
            if (filter == null && !force)
                return;

            for (int i = start; i < end; i++)
            {
                var item = list[i];
                var idx = GetIndexFiltered(item);
                var isIncluded = filter != null ? filter(item, i, this) : true;

                // element is included
                if (isIncluded)
                {
                    // element wasn't included before
                    if (idx < 0)
                    {
                        if (index == Count)
                            InternalAddItem(item);
                        else
                            InternalInsertItem(item, index);
                    }
                    index++;
                }
                // element is not included and was before
                else if (idx >= 0)
                    InternalRemoveItem(item);
            }
        }

        /// <summary>
        /// Get the index in the live list of an object at position.
        /// This will scan back to the beginning of the live list looking for
        /// the closest left neighbour and return the position after that.
        /// </summary>
        /// <param name="position">The index of an object in the unfiltered, sorted list that we want to map to the filtered live list</param>
        /// <param name="list">The unfiltered, sorted list of items</param>
        /// <returns></returns>
        int GetLiveListPivot(int position, IList<T> list)
        {
            var index = -1;
            if (position > 0)
            {
                for (int i = position - 1; i >= 0; i--)
                {
                    index = GetIndexFiltered(list[i]);
                    if (index >= 0)
                    {
                        // found an element to the left of what we want, so now we know the index where to start
                        // manipulating the list
                        index++;
                        break;
                    }
                }
            }

            // there was no element to the left of the one we want, start at the beginning of the live list
            if (index < 0)
                index = 0;
            return index;
        }

        /// <summary>
        /// Adds an item to the filtered list
        /// </summary>
        void InternalAddItem(T item)
        {
            isChanging = true;
            Add(item);
        }

        /// <summary>
        /// Inserts an item into the filtered list
        /// </summary>
        void InternalInsertItem(T item, int position)
        {
            isChanging = true;
            Insert(position, item);
        }

        protected override void InsertItem(int index, T item)
        {
            if (!isChanging)
                throw new InvalidOperationException("Collection cannot be changed manually.");
            isChanging = false;

            filteredIndexCache.Add(item, index);
            UpdateIndexCache(index, Count, Items, filteredIndexCache);
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Removes an item from the filtered list
        /// </summary>
        void InternalRemoveItem(T item)
        {
            int idx;
            // this only happens if the cache is lazy, which is not the case at this time
            if (!filteredIndexCache.TryGetValue(item, out idx))
            {
                Debug.Assert(false);
                return;
            }

            isChanging = true;
            RemoveItem(idx);
        }

        protected override void RemoveItem(int index)
        {
            if (!isChanging)
                throw new InvalidOperationException("Items cannot be removed from the collection except via RemoveItem(T).");
            isChanging = false;
            filteredIndexCache.Remove(this[index]);
            UpdateIndexCache(Count - 1, index, Items, filteredIndexCache);
            base.RemoveItem(index);
        }

        /// <summary>
        /// Moves an item in the filtered list
        /// </summary>
        void InternalMoveItem(int positionFrom, int positionTo)
        {
            isChanging = true;
            positionTo = positionFrom < positionTo ? positionTo - 1 : positionTo;
            Move(positionFrom, positionTo);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (!isChanging)
                throw new InvalidOperationException("Collection cannot be changed manually.");
            isChanging = false;

            if (oldIndex != newIndex)
            {
                UpdateIndexCache(newIndex, oldIndex, Items, filteredIndexCache);
                filteredIndexCache[this[oldIndex]] = newIndex;
            }
            base.MoveItem(oldIndex, newIndex);
        }

        protected override void ClearItems()
        {
            filteredIndexCache.Clear();
            base.ClearItems();
        }

        /// <summary>
        /// The filtered list always has a cache filled up with
        /// all the items that are visible.
        /// </summary>
        int GetIndexFiltered(T item)
        {
            int idx;
            if (filteredIndexCache.TryGetValue(item, out idx))
                return idx;
            return -1;
        }

        /// <summary>
        /// The unfiltered has a lazy cache that gets filled
        /// up when something is looked up.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        int GetIndexUnfiltered(T item)
        {
            int ret;
            if (!sortedIndexCache.TryGetValue(item, out ret))
            {
                ret = original.IndexOf(item);
                if (ret >= 0)
                    sortedIndexCache.Add(item, ret);
            }
            return ret;
        }

        /// <summary>
        /// When items get moved/inserted/deleted, update the indexes in the cache.
        /// If start &lt; end, we're inserting an item and want to shift all the indexes
        /// between start and end to the right (+1)
        /// If start &gt; end, we're removing an item and want to shift all
        /// indexes to the left (-1).
        /// </summary>
        static void UpdateIndexCache(int start, int end, IList<T> list, Dictionary<T, int> indexCache)
        {
            var change = end < start ? -1 : 1;
            for (int i = start; i != end; i += change)
                if (indexCache.ContainsKey(list[i]))
                    indexCache[list[i]] += change;
        }

        static int FindNewPositionForItem(int idx, bool lower, IList<T> list, Func<T, T, int> comparer, Dictionary<T, int> indexCache)
        {
            var i = idx;
            if (lower) // replacing element has lower sorting order, find the correct spot towards the beginning
                for (var pos = i - 1; i > 0 && comparer(list[i], list[pos]) < 0; i--, pos--)
                {
                    Swap(list, i, pos);
                    SwapIndex(list, i, 1, indexCache);
                }

            else // replacing element has higher sorting order, find the correct spot towards the end
                for (var pos = i + 1; i < list.Count - 1 && comparer(list[i], list[pos]) > 0; i++, pos++)
                {
                    Swap(list, i, pos);
                    SwapIndex(list, i, -1, indexCache);
                }
            indexCache[list[i]] = i;
            return i;
        }

        /// <summary>
        /// Swap two elements
        /// </summary>
        static void Swap(IList<T> list, int left, int right)
        {
            var l = list[left];
            list[left] = list[right];
            list[right] = l;
        }

        static void SwapIndex(IList<T> list, int pos, int change, Dictionary<T, int> cache)
        {
            if (cache.ContainsKey(list[pos]))
                cache[list[pos]] += change;
        }

        static int BinarySearch(List<T> list, T item, Func<T, T, int> comparer)
        {
            return list.BinarySearch(item, new LambdaComparer<T>(comparer));
        }

#if DISABLE_REACTIVEUI
        static IScheduler GetScheduler(IScheduler scheduler)
        {
            Dispatcher d = null;
            if (scheduler == null)
                d = Dispatcher.FromThread(Thread.CurrentThread);
            return scheduler ?? (d != null ? new DispatcherScheduler(d) : null as IScheduler) ?? CurrentThreadScheduler.Instance;
        }
#endif

        bool disposed = false;
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    disposed = true;
                    queue = null;
                    disposables.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        struct ActionData
        {
            public static readonly ActionData Default = new ActionData(null);

            readonly public TheAction TheAction;
            readonly public int Position;
            readonly public int OldPosition;
            readonly public int Index;
            readonly public int IndexPivot;
            readonly public bool IsIncluded;
            readonly public T Item;
            readonly public T OldItem;
            readonly public List<T> List;

            public ActionData(ActionData other, int index, int indexPivot)
                : this(other.TheAction, other.List,
                      other.Item, other.OldItem,
                      other.Position, other.OldPosition,
                      index, indexPivot,
                      other.IsIncluded)
            {
            }

            public ActionData(ActionData other, int position)
                : this(other.TheAction, other.List,
                      other.Item, other.OldItem,
                      position, other.OldPosition,
                      other.Index, other.IndexPivot,
                      other.IsIncluded)
            {
            }

            public ActionData(ActionData other, bool isIncluded)
                : this(other.TheAction, other.List,
                      other.Item, other.OldItem,
                      other.Position, other.OldPosition,
                      other.Index, other.IndexPivot,
                      isIncluded)
            {
            }

            public ActionData(TheAction action, List<T> list, T item, T oldItem, int position, int oldPosition)
                : this(action, list,
                      item, oldItem,
                      position, oldPosition,
                      -1, -1, false)
            {
            }

            public ActionData(T item)
                : this(TheAction.None, item)
            {
            }

            public ActionData(TheAction action, T item)
                : this(action, null,
                        item, null,
                        -1, -1,
                        -1, -1, false)
            {
            }

            public ActionData(TheAction action, List<T> list, T item, T oldItem, int position, int oldPosition, int index, int indexPivot, bool isIncluded)
            {
                TheAction = action;
                Item = item;
                OldItem = oldItem;
                Position = position;
                OldPosition = oldPosition;
                List = list;
                Index = index;
                IndexPivot = indexPivot;
                IsIncluded = isIncluded;
            }
        }
    }
}