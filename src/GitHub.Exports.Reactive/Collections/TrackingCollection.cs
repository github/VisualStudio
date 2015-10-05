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
using ReactiveUI;

namespace GitHub.Collections
{
    public class TrackingCollection<T> : ObservableCollection<T>, ITrackingCollection<T>
        where T : ICopyable<T>
    {
        CompositeDisposable disposables = new CompositeDisposable();
        IObservable<T> source;
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

                        if (comparer == null)
                            return Tuple.Create(t, idx, list.Count); // no sorting to be done, just replacing the element in-place

                        if (comparison < 0) // replacing element has lower sorting order, find the correct spot towards the beginning
                        {
                            var i = idx;
                            for (; i > 0 && comparer(list[i], list[i - 1]) < 0; i--)
                                Swap(list, i, i - 1);
                            if (filter == null)
                            {
                                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, old, i, idx));
                            }
                            else
                                return Tuple.Create(t, i, idx + 1);
                        }
                        else if (comparison > 0) // replacing element has higher sorting order, find the correct spot towards the end
                        {
                            var i = idx;
                            for (; i < list.Count - 1 && comparer(list[i], list[i + 1]) > 0; i++)
                                Swap(list, i, i + 1);
                            if (filter == null)
                            {
                                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, old, i, idx));
                            }
                            else
                                return Tuple.Create(t, idx, i + 1);
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
                            return Tuple.Create(t, -1, -1);
                        }
                        list.Insert(i, t);
                        return Tuple.Create(t, i, list.Count);
                    }
                    else
                    {
                        list.Add(t);
                        return Tuple.Create(t, -1, -1);
                    }
                    return Tuple.Create(t, -2, -2);
                })
                .ObserveOn(scheduler)
                .Select(x =>
                {
                    if (x.Item2 > -2)
                        UpdateFilter(x.Item2, x.Item3);
                    return x.Item1;
                })
                .Publish()
                .RefCount();
            return this;
        }

        public void SetComparer(Func<T, T, int> compare)
        {
            comparer = compare;
            UpdateSort();
            UpdateFilter(filter);
        }

        public void SetFilter(Func<T, int, IList<T>, bool> theFilter)
        {
            UpdateFilter(theFilter);
        }

        void UpdateSort()
        {
            if (comparer == null)
                return;

            var list = filter == null ? this : original as IList<T>;
            for (var start = 0; start < Count - 1; start++)
            {
                var idxSmallest = start;
                for (var i = start + 1; i < Count; i++)
                    if (comparer(list[i], list[idxSmallest]) < 0)
                        idxSmallest = i;
                if (idxSmallest == start) continue;
                Swap(list, start, idxSmallest);

                // if there's a filter, then it's going to trigger events
                if (filter == null)
                {
                    OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, this[start], start, idxSmallest));
                }
            }
        }

        void UpdateFilter(int start, int end)
        {
            if (filter == null)
                return;

            if (start > end)
                throw new ArgumentOutOfRangeException(nameof(start), "Start cannot be bigger than End");

            if (start < 0)
            {
                end = original.Count - 1;
                if (filter(original[end], end, this))
                {
                    Add(original[end]);
                }
                return;
            }

            if (start == end)
                return;

            var count = IndexOf(original[start]);
            if (count < 0)
            {
                //count = 0;
                for (int i = start; i != 0; i--)
                {
                    count = IndexOf(original[i]);
                    if (count >= 0)
                    {
                        start = i;
                        break;
                    }
                }
            }

            if (count < 0)
            {
                count = 0;
                start = 0;
            }

            for (int i = start; i < end; i++)
            {
                var idx = IndexOf(original[i]);
                var obj = original[i];
                if (filter(original[i], i, this))
                {
                    if (idx != count)
                    {
                        if (idx >= 0)
                            RemoveItem(idx);

                        if (count > Count)
                            Add(obj);
                        else
                            InsertItem(count, obj);
                    }
                    else
                    {
                        if (idx >= 0)
                            this[idx].CopyFrom(obj);
                    }
                    count++;
                }
                else
                {
                    if (idx >= 0)
                        RemoveItem(idx);
                }
            }
        }

        void UpdateFilter(Func<T, int, IList<T>, bool> filt)
        {
            if (filter != null && filt == null)
            {
                for (int i = 0; i < original.Count; i++)
                {
                    var included = filter(original[i], i, this);
                    if (!included)
                        InsertItem(i, original[i]);
                }
                original = null;
                filter = filt;
                return;
            }

            if (filt == null)
                return;
            if (filter == null)
                original = new List<T>(Items);
            filter = filt;
            UpdateFilter(0, original.Count);
        }

        static void Swap(IList<T> list, int left, int right)
        {
            var l = list[left];
            list[left] = list[right];
            list[right] = l;
        }

        public IDisposable Subscribe()
        {
            disposables.Add(source.Subscribe());
            return this;
        }

        public IDisposable Subscribe(Action<T> onNext, Action onCompleted)
        {
            disposables.Add(source.Subscribe(onNext, onCompleted));
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
