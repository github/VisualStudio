using System;
using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IPullRequestReviewSession
    {
        IPullRequestModel PullRequest { get; }
        ILocalRepositoryModel Repository { get; }

        IReadOnlyList<IPullRequestReviewCommentModel> GetCommentsForFile(string path);
    }
}
