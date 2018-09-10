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

            var checkSuiteModel = new CheckSuiteModel()
            {
                HeadSha = "ed6198c37b13638e902716252b0a17d54bd59e4a",
                CheckRuns = new List<CheckRunModel> { checkRunModel}
            };

            Model= new InlineAnnotationModel(checkSuiteModel, checkRunModel, checkRunAnnotationModel);
        }

        public IInlineAnnotationModel Model { get; }
    }
}