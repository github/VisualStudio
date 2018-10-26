using System;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.Documents;
using ReactiveUI;

namespace GitHub.SampleData.Documents
{
    public class IssueishCommentThreadViewModelDesigner : ViewModelBase, IIssueishCommentThreadViewModel
    {
        public IReadOnlyReactiveList<ICommentViewModel> Comments { get; set; }
        public IActorViewModel CurrentUser { get; } = new ActorViewModelDesigner("grokys");
        public Task DeleteComment(int pullRequestId, int commentId) => Task.CompletedTask;
        public Task EditComment(string id, string body) => Task.CompletedTask;
        public Task InitializeAsync(ActorModel currentUser, IssueishDetailModel model, bool addPlaceholder) => Task.CompletedTask;
        public Task PostComment(string body) => Task.CompletedTask;
    }
}
