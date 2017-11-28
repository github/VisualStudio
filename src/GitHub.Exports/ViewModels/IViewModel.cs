using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Base interface for all view models.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IViewModel : INotifyPropertyChanged
    {
    }
}
