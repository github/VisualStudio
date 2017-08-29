using System;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Interface for view models that have an error state.
    /// </summary>
    public interface IHasErrorState
    {
        /// <summary>
        /// Gets the view model's error message or null if the view model is not in an error state.
        /// </summary>
        string ErrorMessage { get; }
    }
}
