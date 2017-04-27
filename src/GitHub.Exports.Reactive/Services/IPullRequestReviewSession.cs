using System;
using System.Collections.Generic;
using System.Reactive;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IPullRequestReviewSession
    {
        IAccount User { get; }
        IPullRequestModel PullRequest { get; }
        ILocalRepositoryModel Repository { get; }

        IObservable<Unit> Changed { get; }

        void AddComment(IPullRequestReviewCommentModel comment);
        IReadOnlyList<IPullRequestReviewCommentModel> GetCommentsForFile(string path);
    }
}
