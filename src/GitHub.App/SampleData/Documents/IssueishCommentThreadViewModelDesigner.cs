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
        public IActorViewModel CurrentUser { get; } = new ActorViewModelDesigner("grokys");
        public Task InitializeAsync(ActorModel currentUser, IssueishDetailModel model, bool addPlaceholder) => Task.CompletedTask;
        public Task DeleteComment(ICommentViewModel comment) => Task.CompletedTask;
        public Task EditComment(ICommentViewModel comment) => Task.CompletedTask;
        public Task PostComment(ICommentViewModel comment) => Task.CompletedTask;
        public Task CloseIssueish(ICommentViewModel comment) => Task.CompletedTask;
    }
}
