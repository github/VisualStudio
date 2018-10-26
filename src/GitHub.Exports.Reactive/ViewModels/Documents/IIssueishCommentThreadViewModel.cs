using System;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// A thread of issue or pull request comments.
    /// </summary>
    public interface IIssueishCommentThreadViewModel : ICommentThreadViewModel
    {
        /// <summary>
        /// Initializes the view model with data.
        /// </summary>
        /// <param name="currentUser">The currently logged in user.</param>
        /// <param name="model">The issue or pull request model.</param>
        /// <param name="addPlaceholder">
        /// Whether to add a placeholder comment at the end of the thread.
        /// </param>
        Task InitializeAsync(
            ActorModel currentUser,
            IssueishDetailModel model,
            bool addPlaceholder);
    }
}
