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
            var checkRunModel = new CheckRunModel()
            {
                
            };

            Annotations = new[]
            {
                new InlineAnnotationViewModel(new InlineAnnotationModel(checkRunModel, new CheckRunAnnotationModel{ }))
            };
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
