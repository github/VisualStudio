using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace GitHub.Collections
{
    public interface IVirtualizingListSource<T> : IDisposable, INotifyPropertyChanged
    {
        bool IsLoading { get; }
        int PageSize { get; }
        Task<int> GetCount();
        Task<IReadOnlyList<T>> GetPage(int pageNumber);
    }
}