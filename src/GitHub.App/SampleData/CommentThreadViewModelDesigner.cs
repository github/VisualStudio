using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.SampleData
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    public class CommentThreadViewModelDesigner : ViewModelBase, ICommentThreadViewModel
    {
        public CommentThreadViewModelDesigner()
        {
            Comments = new ReactiveList<ICommentViewModel>(){new CommentViewModelDesigner()
            {
                Author = new ActorViewModel{ Login = "shana"},
                Body = "You can use a `CompositeDisposable` type here, it's designed to handle disposables in an optimal way (you can just call `Dispose()` on it and it will handle disposing everything it holds)."
            }};

        }

        public IReadOnlyReactiveList<ICommentViewModel> Comments { get; }
            = new ReactiveList<ICommentViewModel>();

        public IActorViewModel CurrentUser { get; set; }
            = new ActorViewModel { Login = "shana" };

        public Task DeleteComment(ICommentViewModel comment) => Task.CompletedTask;
        public Task EditComment(ICommentViewModel comment) => Task.CompletedTask;
        public Task PostComment(ICommentViewModel comment) => Task.CompletedTask;
    }
}
