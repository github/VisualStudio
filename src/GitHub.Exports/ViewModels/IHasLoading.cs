using System;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Interface for view models that have a busy state.
    /// </summary>
    /// <remarks>
    /// <see cref="IHasLoading"/> is similar to <see cref="IHasBusy"/> but they represent
    /// different states:
    /// - When <see cref="IHasLoading.IsLoading"/> is true: There is no data to display.
    /// - When <see cref="IHasBusy.IsBusy"/> is true: There is data to display but that data is
    /// being updated or is in the process of being loaded.
    /// </remarks>
    public interface IHasLoading
    {
        bool IsLoading { get; }
    }
}
