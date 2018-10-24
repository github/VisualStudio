using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.SampleData
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    public class CommentThreadViewModelDesigner : ICommentThreadViewModel
    {
        public CommentThreadViewModelDesigner()
        {

            var checkRunAnnotationModel1 = new CheckRunAnnotationModel
            {
                AnnotationLevel = CheckAnnotationLevel.Failure,
                Path = "SomeFile.cs",
                EndLine = 12,
                StartLine = 12,
                Message = "CS12345: ; expected",
                Title = "CS12345"
            };

            var checkRunAnnotationModel2 = new CheckRunAnnotationModel
            {
                AnnotationLevel = CheckAnnotationLevel.Warning,
                Path = "SomeFile.cs",
                EndLine = 12,
                StartLine = 12,
                Message = "CS12345: ; expected",
                Title = "CS12345"
            };

            var checkRunAnnotationModel3 = new CheckRunAnnotationModel
            {
                AnnotationLevel = CheckAnnotationLevel.Notice,
                Path = "SomeFile.cs",
                EndLine = 12,
                StartLine = 12,
                Message = "CS12345: ; expected",
                Title = "CS12345"
            };

            var checkRunModel =
                new CheckRunModel
                {
                    Annotations = new List<CheckRunAnnotationModel> { checkRunAnnotationModel1, checkRunAnnotationModel2 },
                    Name = "MSBuildLog Analyzer"
                };

            var checkSuiteModel = new CheckSuiteModel()
            {
                HeadSha = "ed6198c37b13638e902716252b0a17d54bd59e4a",
                CheckRuns = new List<CheckRunModel> { checkRunModel },
                ApplicationName = "My Awesome Check Suite"
            };

            Annotations = new[]
            {
                new InlineAnnotationViewModel(new InlineAnnotationModel(checkSuiteModel, checkRunModel, checkRunAnnotationModel1)),
                new InlineAnnotationViewModel(new InlineAnnotationModel(checkSuiteModel, checkRunModel, checkRunAnnotationModel2)),
                new InlineAnnotationViewModel(new InlineAnnotationModel(checkSuiteModel, checkRunModel, checkRunAnnotationModel3)),
            };

            Comments = new ObservableCollection<ICommentViewModel>(){new CommentViewModelDesigner()
            {
                Author = new ActorViewModel{ Login = "shana"},
                Body = "You can use a `CompositeDisposable` type here, it's designed to handle disposables in an optimal way (you can just call `Dispose()` on it and it will handle disposing everything it holds)."
            }};

        }

        public ObservableCollection<ICommentViewModel> Comments { get; }
            = new ObservableCollection<ICommentViewModel>();

        public IReadOnlyList<IInlineAnnotationViewModel> Annotations { get; }

        public IActorViewModel CurrentUser { get; set; }
            = new ActorViewModel { Login = "shana" };

        public ReactiveCommand<string, Unit> PostComment { get; }
        public ReactiveCommand<Tuple<string, string>, Unit> EditComment { get; }
        public ReactiveCommand<Tuple<int, int>, Unit> DeleteComment { get; }
    }
}
