using GitHub.Models;

namespace GitHub.InlineReviews.ViewModels
{
    /// <inheritdoc />
    public class InlineAnnotationViewModel: IInlineAnnotationViewModel
    {
        /// <inheritdoc />
        public IInlineAnnotationModel Model { get; }

        /// <summary>
        /// Initializes a <see cref="InlineAnnotationViewModel"/>.
        /// </summary>
        /// <param name="model">The inline annotation model.</param>
        public InlineAnnotationViewModel(IInlineAnnotationModel model)
        {
            Model = model;
        }
    }
}