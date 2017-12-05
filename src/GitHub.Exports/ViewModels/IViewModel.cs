using System;
using System.ComponentModel;
using GitHub.UI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Base interface for all view models.
    /// </summary>
    public interface IViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <param name="data">An object containing the related view and the data to load.</param>
        void Initialize(ViewWithData data);
    }
}
