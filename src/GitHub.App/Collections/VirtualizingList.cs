using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GitHub.Logging;
using Serilog;

#pragma warning disable CA1010 // Collections should implement generic interface
#pragma warning disable CA1033 // Interface methods should be callable by child types
#pragma warning disable CA1710 // Identifiers should have correct suffix

namespace GitHub.Collections
{
    /// <summary>
    /// A virtualizing list that loads data only when needed.
    /// </summary>
    /// <typeparam name="T">The list item type.</typeparam>
    /// <remarks>
    /// This class exposes a read-only list where the data is fetched as needed. When the indexer
    /// getter is called, if the requested item is not yet available it calls the associated 
    /// <see cref="IVirtualizingListSource{T}"/> to load the page of data containing the requested
    /// item. While the data is being read, <see cref="Placeholder"/> is returned and when the
    /// data is read <see cref="CollectionChanged"/> is raised.
    /// 
    /// Note that this implementation currently represents the minimum required for interaction
    /// with WPF and as such many members are not yet implemented. In addition, if filtering is
    /// required in the UI then the collection can be wrapped in a
    /// <see cref="VirtualizingListCollectionView{T}"/>.
    /// </remarks>
    public class VirtualizingList<T> : IReadOnlyList<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        static readonly ILogger log = LogManager.ForContext<VirtualizingList<T>>();
        readonly Dictionary<int, IReadOnlyList<T>> pages = new Dictionary<int, IReadOnlyList<T>>();
        readonly IVirtualizingListSource<T> source;
        readonly IList<T> emptyPage;
        readonly IReadOnlyList<T> placeholderPage;
        readonly Dispatcher dispatcher;
        int? count;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualizingList{T}"/> class.
        /// </summary>
        /// <param name="source">The list source.</param>
        /// <param name="placeholder">The placeholder item.</param>
        public VirtualizingList(
            IVirtualizingListSource<T> source,
            T placeholder)
        {
            this.source = source;
            Placeholder = placeholder;
            emptyPage = Enumerable.Repeat(default(T), PageSize).ToList();
            placeholderPage = Enumerable.Repeat(placeholder, PageSize).ToList();
            dispatcher = Application.Current?.Dispatcher;
        }

        /// <summary>
        /// Gets an item by index.
        /// </summary>
        /// <param name="index">The index of the item.</param>
        /// <returns>The item, or <see cref="Placeholder"/> if the item is not yet loaded.</returns>
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

                    if (pages.TryGetValue(pageNumber, out page))
                    {
                        return page[pageIndex];
                    }
                    else
                    {
                        return placeholderPage[0];
                    }
                }
            }

            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the total count of the collection, including not-yet-loaded items.
        /// </summary>
        /// <remarks>
        /// If the count has not yet been loaded, this will return 0 and then raise a
        /// <see cref="PropertyChanged"/> event when the count is loaded.
        /// </remarks>
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

        /// <summary>
        /// Gets the placeholder item that will be displayed while an item is loading.
        /// </summary>
        public T Placeholder { get; }

        /// <summary>
        /// Gets the loaded pages of data.
        /// </summary>
        public IReadOnlyDictionary<int, IReadOnlyList<T>> Pages => pages;

        /// <summary>
        /// Gets the page size of the associated <see cref="IVirtualizingListSource{T}"/>.
        /// </summary>
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
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ErrorEventArgs> InitializationError;

        public IEnumerator<T> GetEnumerator()
        {
            var i = 0;
            while (i < Count) yield return this[i++];
        }

        int IList.Add(object value) => throw new NotImplementedException();
        void IList.Clear() => throw new NotImplementedException();
        bool IList.Contains(object value) => throw new NotImplementedException();
        int IList.IndexOf(object value) => throw new NotImplementedException();
        void IList.Insert(int index, object value) => throw new NotImplementedException();
        void IList.Remove(object value) => throw new NotImplementedException();
        void IList.RemoveAt(int index) => throw new NotImplementedException();
        void ICollection.CopyTo(Array array, int index) => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void LoadCount()
        {
            dispatcher?.VerifyAccess();

            try
            {
                var countTask = source.GetCount();

                if (countTask.IsCompleted)
                {
                    // Don't send a Reset if the count is available immediately, as this causes
                    // a NullReferenceException in ListCollectionView.
                    count = countTask.Result;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                }
                else
                {
                    countTask.ContinueWith(x =>
                    {
                        if (x.IsFaulted)
                        {
                            RaiseInitializationError(x.Exception);
                        }
                        else if (!x.IsCanceled)
                        {
                            count = x.Result;
                            SendReset();
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
            catch (Exception ex)
            {
                RaiseInitializationError(ex);
                log.Error(ex, "Error loading virtualizing list count");
            }
        }

        async void LoadPage(int number)
        {
            dispatcher?.VerifyAccess();

            try
            {
                pages.Add(number, placeholderPage);
                var page = await source.GetPage(number);

                if (page != null)
                {
                    pages[number] = page;
                    SendReset();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error loading virtualizing list page {Number}", number);
                pages.Remove(number);
            }
        }

        void RaiseInitializationError(Exception e)
        {
            if (InitializationError != null)
            {
                if (e is AggregateException ae)
                {
                    e = ae = ae.Flatten();

                    if (ae.InnerExceptions.Count == 1)
                    {
                        e = ae.InnerException;
                    }
                }

                InitializationError(this, new ErrorEventArgs(e));
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