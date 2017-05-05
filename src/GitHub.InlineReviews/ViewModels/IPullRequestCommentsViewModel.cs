using System.Collections.ObjectModel;
using GitHub.Models;

namespace GitHub.InlineReviews.ViewModels
{
    interface IPullRequestCommentsViewModel
    {
        IRepositoryModel Repository { get; }
        int Number { get; }
        string Title { get; }
        ICommentThreadViewModel Conversation { get; }
        ObservableCollection<IDiffCommentThreadViewModel> FileComments { get; }
    }
}