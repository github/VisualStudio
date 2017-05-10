using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.Services
{
    /// <summary>
    /// A pull request session.
    /// </summary>
    /// <remarks>
    /// A pull request session represents the real-time state of a pull request in the IDE.
    /// It takes the pull request model and updates according to the current state of the
    /// repository on disk and in the editor.
    /// </remarks>
    public interface IPullRequestSession
    {
        IAccount User { get; }
        IPullRequestModel PullRequest { get; }
        ILocalRepositoryModel Repository { get; }

        IObservable<Unit> Changed { get; }

        void AddComment(IPullRequestReviewCommentModel comment);
        Task<IReactiveList<IPullRequestSessionFile>> GetAllFiles();
        IReadOnlyList<IPullRequestReviewCommentModel> GetCommentsForFile(string path);
        Task<IPullRequestSessionFile> GetFile(string path);
    }
}
