using GitHub.Models;
using GitHub.ViewModels;

namespace GitHub.ViewModels
{
    /// <inheritdoc />
    public class InlineAnnotationViewModel: IInlineAnnotationViewModel
    {
        /// <inheritdoc />
        public InlineAnnotationModel Model { get; }

        /// <summary>
        /// Initializes a <see cref="InlineAnnotationViewModel"/>.
        /// </summary>
        /// <param name="model">The inline annotation model.</param>
        public InlineAnnotationViewModel(InlineAnnotationModel model)
        {
            Model = model;
        }
    }
}