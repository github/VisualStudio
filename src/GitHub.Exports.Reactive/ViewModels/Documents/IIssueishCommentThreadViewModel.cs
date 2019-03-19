using System;
using System.Threading.Tasks;

namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// A thread of issue or pull request comments.
    /// </summary>
    public interface IIssueishCommentThreadViewModel : ICommentThreadViewModel
    {
        /// <summary>
        /// Called by a comment in the thread to close the issue or pull request.
        /// </summary>
        /// <param name="body">The comment requesting the close.</param>
        Task CloseOrReopen(ICommentViewModel comment);
    }
}
