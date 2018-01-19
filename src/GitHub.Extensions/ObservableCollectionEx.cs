using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GitHub.Extensions
{
    /// <summary>
    /// A non-braindead way to expose a read-only view on an <see cref="ObservableCollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <remarks>
    /// <see cref="ReadOnlyObservableCollection{T}"/> fails in its only purpose by Not. Freaking.
    /// Exposing. INotifyCollectionChanged. We define our own <see cref="IReadOnlyObservableCollection{T}"/>
    /// type and use this class to expose it. Seriously.
    /// </remarks>
    public class ObservableCollectionEx<T> : ObservableCollection<T>, IReadOnlyObservableCollection<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionEx{T}"/> class.
        /// </summary>
        public ObservableCollectionEx()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionEx{T}"/> class that
        /// contains elements copied from the specified list.
        /// </summary>
        public ObservableCollectionEx(List<T> list)
            : base(list)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionEx{T}"/> class that
        /// contains elements copied from the specified collection.
        /// </summary>
        public ObservableCollectionEx(IEnumerable<T> collection)
            : base(collection)
        {
        }
    }
}