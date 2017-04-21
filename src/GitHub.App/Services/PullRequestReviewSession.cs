using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitHub.Models;

namespace GitHub.Services
{
    public class PullRequestReviewSession : IPullRequestReviewSession
    {
        static readonly List<IPullRequestReviewCommentModel> Empty = new List<IPullRequestReviewCommentModel>();
        Dictionary<string, List<IPullRequestReviewCommentModel>> comments;

        public PullRequestReviewSession(
            IPullRequestModel pullRequest,
            ILocalRepositoryModel repository)
        {
            PullRequest = pullRequest;
            Repository = repository;
            this.comments = pullRequest.Comments
                .OrderBy(x => x.Id)
                .GroupBy(x => x.Path)
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        public IPullRequestModel PullRequest { get; }
        public ILocalRepositoryModel Repository { get; }

        public IReadOnlyList<IPullRequestReviewCommentModel> GetCommentsForFile(string path)
        {
            List<IPullRequestReviewCommentModel> result;
            path = path.Replace('\\', '/');
            return comments.TryGetValue(path, out result) ? result : Empty;
        }
    }
}
