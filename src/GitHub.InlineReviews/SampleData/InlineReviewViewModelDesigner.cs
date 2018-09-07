using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.InlineReviews.SampleData
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    class InlineReviewViewModelDesigner : IInlineReviewViewModel
    {
        public InlineReviewViewModelDesigner()
        {

            var checkRunAnnotationModel1 = new CheckRunAnnotationModel
            {
                AnnotationLevel = CheckAnnotationLevel.Failure,
                Filename = "SomeFile.cs",
                EndLine = 12,
                StartLine = 12,
                Message = "CS12345: ; expected",
                Title = "CS12345"
            };

            var checkRunAnnotationModel2 = new CheckRunAnnotationModel
            {
                AnnotationLevel = CheckAnnotationLevel.Warning,
                Filename = "SomeFile.cs",
                EndLine = 12,
                StartLine = 12,
                Message = "CS12345: ; expected",
                Title = "CS12345"
            };

            var checkRunAnnotationModel3 = new CheckRunAnnotationModel
            {
                AnnotationLevel = CheckAnnotationLevel.Notice,
                Filename = "SomeFile.cs",
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

            Annotations = new[]
            {
                new InlineAnnotationViewModel(new InlineAnnotationModel(checkRunModel, checkRunAnnotationModel1)),
                new InlineAnnotationViewModel(new InlineAnnotationModel(checkRunModel, checkRunAnnotationModel2)),
                new InlineAnnotationViewModel(new InlineAnnotationModel(checkRunModel, checkRunAnnotationModel3)),
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

        public ReactiveCommand<Unit> PostComment { get; }
        public ReactiveCommand<Unit> EditComment { get; }
        public ReactiveCommand<Unit> DeleteComment { get; }
    }
}
