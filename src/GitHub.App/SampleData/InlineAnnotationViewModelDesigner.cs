using System.Collections.Generic;
using GitHub.Models;
using GitHub.ViewModels;

namespace GitHub.SampleData
{
    public class InlineAnnotationViewModelDesigner : IInlineAnnotationViewModel
    {
        public InlineAnnotationViewModelDesigner()
        {
            var checkRunAnnotationModel = new CheckRunAnnotationModel
            {
                AnnotationLevel = CheckAnnotationLevel.Failure,
                Path = "SomeFile.cs",
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
                ApplicationName = "Fake Check Suite",
                HeadSha = "ed6198c37b13638e902716252b0a17d54bd59e4a",
                CheckRuns = new List<CheckRunModel> { checkRunModel}
            };

            Model= new InlineAnnotationModel(checkSuiteModel, checkRunModel, checkRunAnnotationModel);
        }

        public InlineAnnotationModel Model { get; }
    }
}