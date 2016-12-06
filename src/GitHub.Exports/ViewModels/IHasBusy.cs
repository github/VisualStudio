using System;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Interface for view models that have a busy state.
    /// </summary>
    public interface IHasBusy
    {
        bool IsBusy { get; }
    }
}
