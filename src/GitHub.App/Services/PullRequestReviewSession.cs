using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitHub.Models;

namespace GitHub.Services
{
    public class PullRequestReviewSession : IPullRequestReviewSession
    {
        List<IPullRequestReviewCommentModel> comments;

        public PullRequestReviewSession(
            ILocalRepositoryModel repository,
            IEnumerable<IPullRequestReviewCommentModel> comments)
        {
            Repository = repository;
            this.comments = new List<IPullRequestReviewCommentModel>(comments);
        }

        public ILocalRepositoryModel Repository { get; }

        public IEnumerable<IPullRequestReviewCommentModel> GetCommentsForFile(string filePath)
        {
            var relativePath = RootedPathToRelativePath(filePath).Replace('\\', '/');
            return comments.Where(x => x.Path == relativePath);
        }

        string RootedPathToRelativePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                var repoRoot = Repository.LocalPath;

                if (path.StartsWith(repoRoot) && path.Length > repoRoot.Length + 1)
                {
                    return path.Substring(repoRoot.Length + 1);
                }
            }

            return null;
        }
    }
}
