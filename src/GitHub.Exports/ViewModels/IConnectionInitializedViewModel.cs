using System;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Represents a view model that requires initialization with a connection.
    /// </summary>
    public interface IConnectionInitializedViewModel : IViewModel
    {
        /// <summary>
        /// Initializes the view model with the specified connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>A task tracking the initialization.</returns>
        Task InitializeAsync(IConnection connection);
    }
}
