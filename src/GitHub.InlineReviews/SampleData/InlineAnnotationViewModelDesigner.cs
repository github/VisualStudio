using System.Collections.Generic;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;

namespace GitHub.InlineReviews.SampleData
{
    public class InlineAnnotationViewModelDesigner : IInlineAnnotationViewModel
    {
        public InlineAnnotationViewModelDesigner()
        {
            var checkRunAnnotationModel = new CheckRunAnnotationModel
            {
                AnnotationLevel = CheckAnnotationLevel.Failure,
                Filename = "SomeFile.cs",
                EndLine = 12,
                StartLine = 12,
                Message = "Some Error Message",
                Title = "CS12345"
            };

            var checkRunModel =
                new CheckRunModel
                {
                    Annotations = new List<CheckRunAnnotationModel> {checkRunAnnotationModel},
                    Name = "Fake Check Run"
                };

            Model= new InlineAnnotationModel(checkRunModel, checkRunAnnotationModel);
        }

        public IInlineAnnotationModel Model { get; }
    }
}