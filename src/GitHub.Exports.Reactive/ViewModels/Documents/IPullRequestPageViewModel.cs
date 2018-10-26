using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// View model for displaying a pull request in a document window.
    /// </summary>
    public interface IPullRequestPageViewModel : IPullRequestViewModelBase
    {
        /// <summary>
        /// Gets the pull request's comment thread.
        /// </summary>
        IIssueishCommentThreadViewModel Thread { get; }

        /// <summary>
        /// Initializes the view model with data.
        /// </summary>
        /// <param name="currentUser">The currently logged in user.</param>
        /// <param name="model">The pull request model.</param>
        Task InitializeAsync(ActorModel currentUser, PullRequestDetailModel model);
    }
}