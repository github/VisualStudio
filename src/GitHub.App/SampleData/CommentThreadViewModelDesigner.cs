using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.SampleData
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    public class CommentThreadViewModelDesigner : ViewModelBase, ICommentThreadViewModel
    {
        public IReadOnlyReactiveList<ICommentViewModel> Comments { get; }
            = new ReactiveList<ICommentViewModel>();

        public IActorViewModel CurrentUser { get; set; }
            = new ActorViewModel { Login = "shana" };

        public Task DeleteComment(int pullRequestId, int commentId) => Task.CompletedTask;
        public Task EditComment(string id, string body) => Task.CompletedTask;
        public Task PostComment(string body) => Task.CompletedTask;
    }
}
