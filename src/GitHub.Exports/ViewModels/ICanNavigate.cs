using System;
using GitHub.UI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Interface for view models in a navigable panel such as the GitHub pane which can signal
    /// to navigate to another page.
    /// </summary>
    public interface ICanNavigate
    {
        /// <summary>
        /// Gets an observable which is signalled with the page to navigate to.
        /// </summary>
        IObservable<ViewWithData> Navigate { get; }
    }
}
