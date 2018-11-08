using GitHub.Models;

namespace GitHub.ViewModels
{
    /// <summary>
    /// A view model that represents a single inline annotation.
    /// </summary>
    public interface IInlineAnnotationViewModel
    {
        /// <summary>
        /// Gets the inline annotation model.
        /// </summary>
        InlineAnnotationModel Model { get; }
    }
}