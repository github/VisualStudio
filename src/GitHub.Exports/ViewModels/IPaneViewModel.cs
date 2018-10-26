using System;
using System.Threading.Tasks;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Represents the top-level content in a Visual Studio ToolWindowPane.
    /// </summary>
    public interface IPaneViewModel : IViewModel
    {
        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <param name="paneServiceProvider">
        /// The service provider for the containing ToolWindowPane.
        /// </param>
        Task InitializeAsync(IServiceProvider paneServiceProvider);
    }
}