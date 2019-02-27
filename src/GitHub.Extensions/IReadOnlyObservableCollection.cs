using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace GitHub.Extensions
{
    /// <summary>
    /// Represents a read-only interface to an <see cref="ObservableCollection{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public interface IReadOnlyObservableCollection<T> : IReadOnlyList<T>,
        INotifyCollectionChanged,
        INotifyPropertyChanged
    {
    }
}