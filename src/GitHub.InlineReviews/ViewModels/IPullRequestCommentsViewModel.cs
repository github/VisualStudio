using System.Collections.ObjectModel;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    interface IPullRequestCommentsViewModel
    {
        IRepositoryModel Repository { get; }
        int Number { get; }
        string Title { get; }
        ICommentThreadViewModel Conversation { get; }
        IReactiveList<IDiffCommentThreadViewModel> FileComments { get; }
    }
}