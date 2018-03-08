using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Data;

namespace GitHub.Collections
{
    public class VirtualizingListCollectionView<T> : CollectionView, IList
    {
        readonly VirtualizingList<T> inner;
        List<int> filtered;

        public VirtualizingListCollectionView(VirtualizingList<T> inner)
            : base(inner)
        {
            this.inner = inner;
        }

        public override int Count => filtered?.Count ?? inner.Count;
        public override bool IsEmpty => Count == 0;

        bool IList.IsReadOnly => true;
        bool IList.IsFixedSize => false;
        object ICollection.SyncRoot => null;
        bool ICollection.IsSynchronized => false;

        object IList.this[int index]
        {
            get { return GetItemAt(index); }
            set { throw new NotImplementedException(); }
        }

        public override object GetItemAt(int index)
        {
            if (filtered == null)
            {
                return inner[index];
            }
            else
            {
                return inner[filtered[index]];
            }
        }

        int IList.Add(object value) { throw new NotSupportedException(); }
        bool IList.Contains(object value) { throw new NotImplementedException(); }
        void IList.Clear() { throw new NotSupportedException(); }
        int IList.IndexOf(object value) { throw new NotImplementedException(); }
        void IList.Insert(int index, object value) { throw new NotSupportedException(); }
        void IList.Remove(object value) { throw new NotSupportedException(); }
        void IList.RemoveAt(int index) { throw new NotSupportedException(); }
        void ICollection.CopyTo(Array array, int index) { throw new NotImplementedException(); }

        protected override void RefreshOverride()
        {
            if (Filter != null)
            {
                var result = new List<int>();
                var count = inner.Count;

                for (var i = 0; i < count / inner.PageSize; ++i)
                {
                    IReadOnlyList<T> page;

                    if (inner.Pages.TryGetValue(i, out page))
                    {
                        var j = 0;

                        foreach (var item in page)
                        {
                            if (Equals(item, inner.Placeholder) || Filter(item))
                            {
                                result.Add((i * inner.PageSize) + j);
                            }

                            ++j;
                        }
                    }
                    else
                    {
                        for (var j = 0; j < inner.PageSize; ++j)
                        {
                            result.Add((i * inner.PageSize) + j);
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
