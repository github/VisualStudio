using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace GitHub.Collections
{
    public class VirtualizingList<T> : IReadOnlyList<T>, IList, INotifyCollectionChanged
    {
        readonly Dictionary<int, IReadOnlyList<T>> pages = new Dictionary<int, IReadOnlyList<T>>();
        readonly IVirtualizingListSource<T> source;
        readonly IList<T> emptyPage;
        readonly IReadOnlyList<T> placeholderPage;
        readonly Dispatcher dispatcher;
        int? count;

        public VirtualizingList(
            IVirtualizingListSource<T> source,
            T placeholder)
        {
            this.source = source;
            Placeholder = placeholder;
            emptyPage = Enumerable.Repeat(default(T), PageSize).ToList();
            placeholderPage = Enumerable.Repeat(placeholder, PageSize).ToList();
            dispatcher = Application.Current.Dispatcher;
        }

        public T this[int index]
        {
            get
            {
                var pageNumber = index / PageSize;
                var pageIndex = index % PageSize;
                IReadOnlyList<T> page;

                if (pages.TryGetValue(pageNumber, out page))
                {
                    return page[pageIndex];
                }
                else
                {
                    LoadPage(pageNumber);
                    return placeholderPage[0];
                }
            }

            set { throw new NotImplementedException(); }
        }

        public int Count
        {
            get
            {
                if (!count.HasValue)
                {
                    count = 0;
                    LoadCount();
                }

                return count.Value;
            }
        }

        public T Placeholder { get; }
        public IReadOnlyDictionary<int, IReadOnlyList<T>> Pages => pages;
        public int PageSize => source.PageSize;

        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        }

        bool IList.IsReadOnly => true;
        bool IList.IsFixedSize => false;
        int ICollection.Count => Count;
        object ICollection.SyncRoot => null;
        bool ICollection.IsSynchronized => false;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IEnumerator<T> GetEnumerator()
        {
            var i = 0;
            while (i < Count) yield return this[i++];
        }

        int IList.Add(object value)
        {
            throw new NotImplementedException();
        }

        void IList.Clear()
        {
            throw new NotImplementedException();
        }

        bool IList.Contains(object value)
        {
            throw new NotImplementedException();
        }

        int IList.IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void LoadCount()
        {
            dispatcher.VerifyAccess();

            try
            {
                var countTask = source.GetCount();

                if (countTask.IsCompleted)
                {
                    // Don't send a Reset if the count is available immediately, as this causes
                    // a NullReferenceException in ListCollectionView.
                    count = countTask.Result;
                }
                else
                {
                    countTask.ContinueWith(x =>
                    {
                        count = x.Result;
                        SendReset();
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
            catch (Exception)
            {
                // Handle exception.
            }
        }

        async void LoadPage(int number)
        {
            dispatcher.VerifyAccess();

            try
            {
                pages.Add(number, placeholderPage);
                var page = await source.GetPage(number);
                pages[number] = page;
                SendReset();
            }
            catch (Exception)
            {
                // Handle exception.
            }
        }

        void SendReset()
        {
            // ListCollectionView (which is used internally by the WPF list controls) doesn't
            // support multi-item Replace notifications, so sending a Reset is actually the
            // best thing we can do to notify of items being loaded.
            CollectionChanged?.Invoke(
                this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}