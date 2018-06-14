using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub.Collections
{
    public interface IVirtualizingListSource<T>
    {
        int PageSize { get; }
        Task<int> GetCount();
        Task<IReadOnlyList<T>> GetPage(int pageNumber);
    }
}