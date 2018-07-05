using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace GitHub.Collections
{
    /// <summary>
    /// A loader for a virtualizing list.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <remarks>
    /// This interface is used by the <see cref="VirtualizingList{T}"/> class to load pages of data.
    /// </remarks>
    public interface IVirtualizingListSource<T> : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets a value that indicates where loading is in progress.
        /// </summary>
        bool IsLoading { get; }

        /// <summary>
        /// Gets the page size of the list source.
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Gets the total number of items in the list.
        /// </summary>
        /// <returns>A task returning the count.</returns>
        Task<int> GetCount();

        /// <summary>
        /// Gets the numbered page of items.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <returns>A task returning the page contents.</returns>
        Task<IReadOnlyList<T>> GetPage(int pageNumber);
    }
}