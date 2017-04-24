using System;
using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IPullRequestReviewSession
    {
        IAccount User { get; }
        IPullRequestModel PullRequest { get; }
        ILocalRepositoryModel Repository { get; }
        string CompareViewHackPath { get; }

        IReadOnlyList<IPullRequestReviewCommentModel> GetCommentsForFile(string path);
        IDisposable OpeningCompareViewHack(string path);
    }
}
