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
        public ObservableCollection<ICommentViewModel> Comments { get; }
            = new ObservableCollection<ICommentViewModel>();

        public IActorViewModel CurrentUser { get; set; }
            = new ActorViewModel { Login = "shana" };

        public ReactiveCommand<Unit> PostComment { get; }
        public ReactiveCommand<Unit> EditComment { get; }
        public ReactiveCommand<Unit> DeleteComment { get; }
    }
}
