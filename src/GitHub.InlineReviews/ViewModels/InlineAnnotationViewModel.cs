using GitHub.Models;

namespace GitHub.InlineReviews.ViewModels
{
    public class InlineAnnotationViewModel: IInlineAnnotationViewModel
    {
        public IInlineAnnotationModel Model { get; }

        public InlineAnnotationViewModel(IInlineAnnotationModel model)
        {
            Model = model;
        }
    }
}