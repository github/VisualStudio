using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Data;

#pragma warning disable CA1010 // Collections should implement generic interface
#pragma warning disable CA1033 // Interface methods should be callable by child types
#pragma warning disable CA1710 // Identifiers should have correct suffix

namespace GitHub.Collections
{
    /// <summary>
    /// A <see cref="CollectionView"/> that adds filtering to a <see cref="VirtualizingList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    public class VirtualizingListCollectionView<T> : CollectionView, IList
    {
        List<int> filtered;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualizingListCollectionView{T}"/> class.
        /// </summary>
        /// <param name="inner">The inner virtualizing list.</param>
        public VirtualizingListCollectionView(VirtualizingList<T> inner)
            : base(inner)
        {
        }

        /// <summary>
        /// Gets the count of the filtered items.
        /// </summary>
        public override int Count => filtered?.Count ?? Inner.Count;

        /// <inheritdoc/>
        public override bool IsEmpty => Count == 0;

        bool IList.IsReadOnly => true;
        bool IList.IsFixedSize => false;
        object ICollection.SyncRoot => null;
        bool ICollection.IsSynchronized => false;

        /// <summary>
        /// Gets the inner virtualizing list.
        /// </summary>
        protected VirtualizingList<T> Inner => (VirtualizingList<T>)SourceCollection;

        object IList.this[int index]
        {
            get { return GetItemAt(index); }
            set { throw new NotImplementedException(); }
        }

        /// <inheritdoc/>
        public override object GetItemAt(int index)
        {
            if (filtered == null)
            {
                return Inner[index];
            }
            else
            {
                return Inner[filtered[index]];
            }
        }

        int IList.Add(object value) => throw new NotSupportedException();
        bool IList.Contains(object value) => throw new NotImplementedException();
        void IList.Clear() => throw new NotSupportedException();
        int IList.IndexOf(object value) => throw new NotImplementedException();
        void IList.Insert(int index, object value) => throw new NotSupportedException();
        void IList.Remove(object value) => throw new NotSupportedException();
        void IList.RemoveAt(int index) => throw new NotSupportedException();
        void ICollection.CopyTo(Array array, int index) => throw new NotImplementedException();

        protected override void RefreshOverride()
        {
            if (Filter != null)
            {
                var result = new List<int>();
                var count = Inner.Count;
                var pageCount = (int)Math.Ceiling((double)count / Inner.PageSize);

                for (var i = 0; i < pageCount; ++i)
                {
                    IReadOnlyList<T> page;

                    if (Inner.Pages.TryGetValue(i, out page))
                    {
                        var j = 0;

                        foreach (var item in page)
                        {
                            if (Equals(item, Inner.Placeholder) || Filter(item))
                            {
                                result.Add((i * Inner.PageSize) + j);
                            }

                            ++j;
                        }
                    }
                    else
                    {
                        for (var j = 0; j < Inner.PageSize; ++j)
                        {
                            result.Add((i * Inner.PageSize) + j);
                        }
                    }
                }

                filtered = result;
            }
            else
            {
                filtered = null;
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}