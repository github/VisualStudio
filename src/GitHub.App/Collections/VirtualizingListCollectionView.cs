using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Data;

namespace GitHub.Collections
{
    public class VirtualizingListCollectionView<T> : CollectionView
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
